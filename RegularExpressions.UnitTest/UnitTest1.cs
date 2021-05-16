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
            var example = new Example();

            string expected = string.Empty;

            // Arrange
            example.RegexMatch_SearchCode();

            // Act
            string actual = string.Empty;

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}