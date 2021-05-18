using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Utilities
{
    /// <summary>
    ///
    /// </summary>
    public class WebRequestExample
    {
        /// <summary>
        /// https://stackoverflow.com/questions/24478872/post-to-form-in-c-sharp-pass-antiforgerytoken
        /// https://nozzlegear.com/blog/send-and-validate-an-asp-net-antiforgerytoken-as-a-request-header
        /// https://stackoverflow.com/questions/9965410/are-asp-net-mvc-4-beta-editor-templates-safe-against-csrf
        /// https://matthewgladney.com/blog/web-apps/using-cookiecontainer-and-webclient-to-access-asp-mvc-anti-csrf-enabled-websites/
        /// </summary>
        public string apiDepartmentGet()
        {
            string url = "https://localhost:44396/api/Department/Get";
            string loginPageAddress = "https://localhost:44396/Account/Login";
            string username = "admin@site.com";
            string password = "123@eRdBEkfuJGyC6jK";
            string rememberMe = "true";
            string returnUrl = "";
            string antiForgeryToken = "";
            string result = "";

            NameValueCollection loginData = new NameValueCollection();

            int requestTimeout = Convert.ToInt32("100000"); // ~ 100.000 miliseconds ~ 100 seconds
            requestTimeout = requestTimeout < 10000 ? 10000 : requestTimeout;
            CookieContainer container = new CookieContainer();

            try
            {
                // ------------------------------ Request 1 ------------------------------ //
                var request1 = WebRequest.Create(loginPageAddress) as HttpWebRequest;
                request1.Method = "GET";
                request1.CookieContainer = container;
                request1.Timeout = requestTimeout;

                try
                {
                    // ------------------------------ Response 1 ------------------------------ //
                    var response1 = request1.GetResponse() as HttpWebResponse;
                    var stream1 = response1.GetResponseStream();
                    using (var streamReader = new StreamReader(stream1))
                    {
                        HtmlNode.ElementsFlags.Remove("form");
                        string html = streamReader.ReadToEnd();
                        var doc = new HtmlDocument();
                        doc.LoadHtml(html);
                        HtmlNode input = doc.DocumentNode.SelectSingleNode("//*[@name='__RequestVerificationToken']");
                        antiForgeryToken = input.Attributes["value"].Value;
                    }
                    foreach (Cookie cookie in response1.Cookies)
                    {
                        container.Add(cookie);
                    }
                }
                catch (WebException e)
                {
                    if (e.Status == WebExceptionStatus.Timeout)
                    {
                        return string.Empty;
                    }
                    Debug.WriteLine(String.Format("using (HttpWebResponse response1 [...] : ErrorMessage={0}, InnerException={1}", e.Message, (e.InnerException != null) ? e.InnerException.Message : ""));
                    throw;
                }

                //Passing FormData
                loginData.Add("__RequestVerificationToken", antiForgeryToken);
                loginData.Add("Username", username);
                loginData.Add("Password", password);
                loginData.Add("RememberMe", rememberMe);
                loginData.Add("ReturnUrl", returnUrl);

                // ------------------------------ Request 2 ------------------------------ //
                var request2 = (HttpWebRequest)WebRequest.Create(loginPageAddress);
                request2.Method = "POST";
                request2.CookieContainer = container;
                request2.ContentType = "application/x-www-form-urlencoded";
                request2.Timeout = requestTimeout;

                var formData = GenerateQueryString(loginData);
                var formDataToArrayBytes = Encoding.ASCII.GetBytes(formData);
                request2.ContentLength = formDataToArrayBytes.Length;
                var stream2 = request2.GetRequestStream();
                stream2.Write(formDataToArrayBytes, 0, formDataToArrayBytes.Length);
                stream2.Close();

                try
                {
                    // ------------------------------ Response 2 ------------------------------ //
                    var response2 = request2.GetResponse() as HttpWebResponse;
                    response2.Close();
                }
                catch (WebException e)
                {
                    if (e.Status == WebExceptionStatus.Timeout)
                    {
                        return string.Empty;
                    }
                    Debug.WriteLine(String.Format("using (HttpWebResponse response2 [...] : ErrorMessage={0}, InnerException={1}", e.Message, (e.InnerException != null) ? e.InnerException.Message : ""));
                    throw;
                }

                //IMPORTANT
                container = request2.CookieContainer;

                if (container != null)
                {
                    var cookiesCollection = GetAllCookies(container);
                    Debug.WriteLine(cookiesCollection.Count);

                    // ------------------------------ Request 3 ------------------------------ //
                    var request3 = WebRequest.Create(url) as HttpWebRequest;
                    request3.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.134 Safari/537.36";
                    request3.CookieContainer = container;
                    request3.Method = "GET";
                    request3.Accept = "application/xml";
                    request3.Timeout = requestTimeout;

                    try
                    {
                        // ------------------------------ Response 3 ------------------------------ //
                        using (var response3 = request3.GetResponse() as HttpWebResponse)
                        {
                            var stream3 = response3.GetResponseStream();
                            var streamReader = new StreamReader(stream3);
                            result = streamReader.ReadToEnd();
                            File.WriteAllText("api_Department_Get.txt", result, Encoding.UTF8);
                        }
                        return result;
                    }
                    catch (WebException e)
                    {
                        if (e.Status == WebExceptionStatus.Timeout)
                        {
                            return string.Empty;
                        }
                        Debug.WriteLine(String.Format("using (HttpWebResponse response [...] : ErrorMessage={0}, InnerException={1}", e.Message, (e.InnerException != null) ? e.InnerException.Message : ""));
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(String.Format("WebRequest.Create(url={0}) : ErrorMessage={1}, InnerException={2}", url, e.Message, (e.InnerException != null) ? e.InnerException.Message : ""));
                throw;
            }
            return string.Empty;
        }

        public string GenerateQueryString(NameValueCollection collection)
        {
            var array = (from key in collection.AllKeys
                         from value in collection.GetValues(key)
                         select string.Format("{0}={1}", WebUtility.UrlEncode(key), WebUtility.UrlEncode(value))).ToArray();
            return string.Join("&", array);
        }

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }

        public static void WriteFile(string fileName, Stream inputStream)
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!path.EndsWith(@"\")) path += @"\";

            if (File.Exists(Path.Combine(path, fileName)))
                File.Delete(Path.Combine(path, fileName));

            using (FileStream fs = new FileStream(Path.Combine(path, fileName), FileMode.CreateNew, FileAccess.Write))
            {
                CopyStream(inputStream, fs);
            }

            inputStream.Close();
            inputStream.Flush();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="cookies"></param>
        /// <returns></returns>
        public CookieCollection GetCookies(CookieContainer cookies)
        {
            CookieCollection cookieCollection = new CookieCollection();

            Hashtable table = (Hashtable)cookies.GetType().InvokeMember(
                "m_domainTable",
                BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance,
                null,
                cookies,
                new object[] { }
            );

            foreach (var key in table.Keys)
            {
                Uri uri = new Uri(string.Format("http://{0}/", key));

                foreach (Cookie cookie in cookies.GetCookies(uri))
                {
                    Debug.WriteLine("Name = {0} ; Value = {1} ; Domain = {2}",
                        cookie.Name, cookie.Value, cookie.Domain);

                    cookieCollection.Add(cookie);
                }
            }
            return cookieCollection;
        }

        public CookieCollection GetAllCookies(CookieContainer cookieJar)
        {
            CookieCollection cookieCollection = new CookieCollection();

            Hashtable table = (Hashtable)cookieJar.GetType().InvokeMember("m_domainTable",
                                                                            BindingFlags.NonPublic |
                                                                            BindingFlags.GetField |
                                                                            BindingFlags.Instance,
                                                                            null,
                                                                            cookieJar,
                                                                            new object[] { });

            foreach (var tableKey in table.Keys)
            {
                String str_tableKey = (string)tableKey;

                if (str_tableKey[0] == '.')
                {
                    str_tableKey = str_tableKey.Substring(1);
                }

                SortedList list = (SortedList)table[tableKey].GetType().InvokeMember("m_list",
                                                                            BindingFlags.NonPublic |
                                                                            BindingFlags.GetField |
                                                                            BindingFlags.Instance,
                                                                            null,
                                                                            table[tableKey],
                                                                            new object[] { });

                foreach (var listKey in list.Keys)
                {
                    String url = "https://" + str_tableKey + (string)listKey;
                    cookieCollection.Add(cookieJar.GetCookies(new Uri(url)));

                    foreach (Cookie cookie in cookieJar.GetCookies(new Uri(url)))
                    {
                        Debug.WriteLine("Name = {0} ; Value = {1} ; Domain = {2}",
                            cookie.Name, cookie.Value, cookie.Domain);
                    }
                }
            }

            return cookieCollection;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public XDocument TryToGetResponse()
        {
            string url = "https://localhost:44396/api/Department/Get";
            XDocument doc = new XDocument();
            Int32 TimeOutPec = Convert.ToInt32("100000");
            TimeOutPec = TimeOutPec < 10000 ? 10000 : TimeOutPec;
            //Debug.WriteLine(String.Format("Fonction=GetPecFromWs(url={0}) avant WebRequest.Create(url)", url));

            HttpWebRequest webRequest;
            CookieContainer cookies;
            try
            {
                string username = "admin@site.com";
                string password = "123@eRdBEkfuJGyC6jK";
                string rememberMe = "true";
                string returnUrl = "";
                string loginPage = "https://localhost:44396/Account/Login";
                string formParams = string.Format("?Email={0}&Password={1}&RememberMe={2}&ReturnUrl={3}", username, password, rememberMe, returnUrl);

                cookies = new CookieContainer();
                webRequest = WebRequest.Create(loginPage) as HttpWebRequest;
                webRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.134 Safari/537.36";
                webRequest.CookieContainer = cookies;
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.Accept = "application/json";
                webRequest.Method = "POST";
                byte[] bytes = Encoding.ASCII.GetBytes(formParams);
                webRequest.ContentLength = bytes.Length;
                using (Stream loginStream = webRequest.GetRequestStream())
                {
                    loginStream.Write(bytes, 0, bytes.Length);
                }
                using (HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse)
                {
                    webResponse.Dispose();
                }
                if (cookies != null)
                {
                    //TODO: MANH
                    var cookieCollection = GetAllCookies(cookies);

                    //TODO: MANH
                    webRequest = WebRequest.Create(url) as HttpWebRequest;
                    webRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.134 Safari/537.36";
                    webRequest.CookieContainer = cookies;
                    webRequest.Method = "GET";
                    webRequest.Accept = "application/xml";
                    webRequest.Timeout = TimeOutPec;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(String.Format("Erreur dans GetPecFromWs > WebRequest.Create(url={0}) : ErrorMessage={1}, InnerException={2}", url, e.Message, (e.InnerException != null) ? e.InnerException.Message : ""));
                throw;
            }

            Debug.WriteLine("Avant using (HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse) [...] XDocument.Load(webResponse.GetResponseStream())");

            try
            {
                using (HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse)
                {
                    doc = XDocument.Load(webResponse.GetResponseStream());
                    return doc;
                }
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.Timeout)
                {
                    return null;
                }
                Debug.WriteLine(String.Format("Erreur dans GetPecFromWs > using (HttpWebResponse [...] : ErrorMessage={0}, InnerException={1}", e.Message, (e.InnerException != null) ? e.InnerException.Message : ""));
                throw;
            }
        }
    }
}