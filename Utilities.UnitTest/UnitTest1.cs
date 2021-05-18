using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Utilities.UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void WebRequestExample_AfterLoginSuccess_ReturnJson()
        {
            //Setup
            var webRequestExample = new WebRequestExample();

            string expected = string.Empty;

            //Arrange
            webRequestExample.apiDepartmentGet();

            //Act
            string actual = string.Empty;

            //Assert
            Assert.AreEqual(expected, actual);
        }
    }
}