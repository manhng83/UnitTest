using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RegularExpressions.UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Example_RegexMatch_SearchCode_ReturnSuccess()
        {
            // Setup
            var test = new Example();

            // Arrange
            test.RegexMatch_SearchCode();

            // Act
            string timeOfDay = string.Empty;

            // Assert
            Assert.AreEqual(string.Empty, timeOfDay);
        }
    }
}