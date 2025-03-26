using HorrorTracker.Utilities.MathFunctions;
using System.Diagnostics.CodeAnalysis;

namespace HorrorTracker.MSTests.Utilities.MathFunctions
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class SimpleMathFunctionsTests
    {
        [TestMethod]
        public void Add_WhenTwoNumbersAreProvided_ShouldReturnSum()
        {
            // Arrange
            var expectedValue = 4.0m;

            // Act
            var actualValue = SimpleMathFunctions.Add(1.3m, 2.7m);

            // Assert
            Assert.AreEqual(expectedValue, actualValue);
        }

        [TestMethod]
        public void Divide_whenTwoNumbersAreProvided_ShouldReturnQuotient()
        {
            // Arrange
            var expectedValue = 6.0m;

            // Act
            var actualValue = SimpleMathFunctions.Divide(24.0m, 4.0m);

            // Assert
            Assert.AreEqual(expectedValue, actualValue);
        }

        [TestMethod]
        public void Divide_WhenDividingByZero_ShouldReturnDivideByZeroException()
        {
            // Arrange

            // Act

            // Assert
            Assert.ThrowsException<DivideByZeroException>(() => SimpleMathFunctions.Divide(32.87m, 0.0m));
        }
    }
}