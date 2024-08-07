﻿using HorrorTracker.Utilities.Parsing;
using System.Diagnostics.CodeAnalysis;

namespace HorrorTracker.MSTests.Utilities.Parsing
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ParserTests
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private Parser _parser;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [TestInitialize]
        public void Initialize()
        {
            _parser = new Parser();
        }

        [TestMethod]
        public void IsInteger_WhenValueIsAnInteger_ShouldReturnTrue()
        {
            // Arrange
            var stringValue = "123";
            var expectedIntegerValue = 123;

            // Act
            var boolValue = _parser.IsInteger(stringValue, out var actualIntegerValue);

            // Assert
            Assert.IsTrue(boolValue);
            Assert.AreEqual(expectedIntegerValue, actualIntegerValue);
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow("string")]
        public void IsInteger_WhenValueIsNotInteger_ShouldReturnFalse(string stringValue)
        {
            // Arrange
            var expectedIntegerValue = 0;

            // Act
            var boolValue = _parser.IsInteger(stringValue, out var actualIntegerValue);

            // Assert
            Assert.IsFalse(boolValue);
            Assert.AreEqual(expectedIntegerValue, actualIntegerValue);
        }

        [TestMethod]
        public void IsDecimal_WhenValueIsDecimal_ShouldReturnTrue()
        {
            // Arrange
            var value = 12.34M;
            var expectedDecimalValue = 12.34M;

            // Act
            var boolValue = _parser.IsDecimal(value, out var actualDecimalValue);

            // Assert
            Assert.IsTrue(boolValue);
            Assert.AreEqual(expectedDecimalValue, actualDecimalValue);
        }

        [TestMethod]
        public void IsDecimal_WhenValueIsString_ShouldReturnTrue()
        {
            // Arrange
            var value = "12.34";
            var expectedDecimalValue = 12.34M;

            // Act
            var boolValue = _parser.IsDecimal(value, out var actualDecimalValue);

            // Assert
            Assert.IsTrue(boolValue);
            Assert.AreEqual(expectedDecimalValue, actualDecimalValue);
        }

        [TestMethod]
        public void IsDecimal_WhenValueIsNotDecimalOrString_ShouldReturnFalse()
        {
            // Arrange
            var expectedDecimalValue = 0.0M;

            // Act
            var boolValue = _parser.IsDecimal(new object(), out var actualDecimalValue);

            // Assert
            Assert.IsFalse(boolValue);
            Assert.AreEqual(expectedDecimalValue, actualDecimalValue);
        }

        [DataTestMethod]
        [DataRow("string", false)]
        [DataRow("   ", true)]
        [DataRow("", true)]
        [DataRow(null, true)]
        public void StringIsNull_WhenDifferentValuesArePassedIntoMethod_ShouldReturnAppropriateBoolean(string value, bool expectedReturnValue)
        {
            // Arrange

            // Act
            var actualReturnValue = _parser.StringIsNull(value);

            // Assert
            Assert.AreEqual(expectedReturnValue, actualReturnValue);
        }
    }
}