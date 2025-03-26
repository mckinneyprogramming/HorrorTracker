using HorrorTracker.Utilities;

namespace HorrorTracker.MSTests.Utilities
{
    [TestClass]
    public class StringUtilityTests
    {
        [DataTestMethod]
        [DataRow("loomis", "Loomis")]
        [DataRow("Prescott", "Prescott")]
        [DataRow("mYERS", "MYERS")]
        [DataRow("3D", "3D")]
        public void CapitalizeFirstLetter_WhenProvidedWithStringInputs_ShouldCapitalizeFirstLetter(string input, string expectedOutput)
        {
            // Arrange

            // Act
            var actualOutput = StringUtility.CapitalizeFirstLetter(input);

            // Assert
            Assert.AreEqual(expectedOutput, actualOutput);
        }
    }
}