//using System;
//using System.Diagnostics;
//using System.IO;
//using System.Text;
//using System.Text.RegularExpressions;

using System.Linq;

namespace RegularExpressions
{
    /// <summary>
    ///
    /// </summary>
    public class Example
    {
        public static string FileOutputName = "SearchCode.txt";
        public static string SearchText = @"\bFunctions.Get\w*Service\b"; //DELETE ME
        public static string Folder = @"C:\Src\"; //DELETE ME

        /// <summary>
        /// Search all code has text like "Functions.GetXXXService"
        /// </summary>
        public void RegexMatch_SearchCode()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            string folder = Folder; //Folder that include the (Web.config || Starup.cs) of Web Application
            string pattern = SearchText;

            string currentDir = System.Environment.CurrentDirectory;
            string fileName = FileOutputName;
            string filePath = System.IO.Path.Combine(currentDir, fileName);

            System.Collections.Generic.HashSet<string> filesHasFound = new System.Collections.Generic.HashSet<string>();
            System.Collections.Generic.Dictionary<string, string> keyValuePairs = new System.Collections.Generic.Dictionary<string, string>();
            if (System.IO.Directory.Exists(folder))
            {
                string[] files = System.IO.Directory.GetFiles(folder, "*.cs", System.IO.SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; i++)
                {
                    string f = files[i];
                    if (!f.Contains(@"\Controllers\"))
                    {
                        continue;
                    }
                    string[] fContent = System.IO.File.ReadAllLines(f, System.Text.Encoding.UTF8);
                    for (int j = 0; j < fContent.Length; j++)
                    {
                        string line = fContent[j];
                        string lineNoTab = System.Text.RegularExpressions.Regex.Replace(line, "   //", "//");
                        string lineNoSpaceBeforeSlashSlash = System.Text.RegularExpressions.Regex.Replace(lineNoTab, "  //", "//");
                        string input = System.Text.RegularExpressions.Regex.Replace(lineNoSpaceBeforeSlashSlash, " //", "//");

                        System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(input, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                        //Do not item in BLOCK COMMENT CODE
                        //Only files in \Areas\...\Controllers\
                        //           OR
                        //              \Controllers\
                        if (m.Success && !line.Contains("//") && !m.Value.Contains("//"))
                        {
                            string s = $"{f} : ({i},{m.Index})";
                            if (!keyValuePairs.Keys.Contains(f))
                            {
                                string openToEditFile = $"devenv /edit \"{f}\"";
                                filesHasFound.Add(openToEditFile);

                                keyValuePairs.Add(f, System.IO.Path.GetFileName(f));
                                sb.AppendLine();
                                sb.Append(s);
                            }
                            else
                            {
                                sb.Append($" ({i},{m.Index})");
                            }
                            System.Diagnostics.Debug.WriteLine("Found '{0}' at position {1}.", m.Value, m.Index);
                        }
                    }
                }
                //System.IO.File.WriteAllText(filePath, sb.ToString(), System.Text.Encoding.UTF8);
                System.IO.File.WriteAllText(filePath, string.Join(System.Environment.NewLine, filesHasFound), System.Text.Encoding.UTF8);
                System.Diagnostics.Process.Start(@"C:\Program Files \Notepad++\notepad++.exe", filePath);
            }
        }
    }
}