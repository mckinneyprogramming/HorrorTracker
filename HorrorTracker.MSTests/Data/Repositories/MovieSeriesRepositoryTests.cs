﻿using AutoFixture;
using HorrorTracker.Data.Constants.Queries;
using HorrorTracker.Data.Models;
using HorrorTracker.Data.PostgreHelpers.Interfaces;
using Moq;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using HorrorTracker.Utilities.Logging.Interfaces;
using HorrorTracker.Data.Repositories;
using HorrorTracker.MSTests.Shared;

namespace HorrorTracker.MSTests.Data.Repositories
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class MovieSeriesRepositoryTests
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private Mock<IDatabaseConnection> _mockDatabaseConnection;
        private Mock<IDatabaseCommand> _mockDatabaseCommand;
        private Mock<ILoggerService> _mockLoggerService;
        private MovieSeriesRepository _repository;
        private LoggerVerifier _loggerVerifier;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [TestInitialize]
        public void Initialize()
        {
            _mockDatabaseConnection = new Mock<IDatabaseConnection>();
            _mockDatabaseCommand = new Mock<IDatabaseCommand>();
            _mockLoggerService = new Mock<ILoggerService>();
            _repository = new MovieSeriesRepository(_mockDatabaseConnection.Object, _mockLoggerService.Object);
            _loggerVerifier = new LoggerVerifier(_mockLoggerService);
        }

        [TestMethod]
        public void AddMovieSeries_SuccessfulConnectionAndAddition_ShouldReturnGoodStatus()
        {
            // Arrange
            var fixture = new Fixture();
            var movieSeries = fixture.Create<MovieSeries>();
            var expectedReturnStatus = 1;
            _mockDatabaseConnection.Setup(db => db.Open());
            _mockDatabaseCommand.Setup(cmd => cmd.ExecuteScalar()).Returns(expectedReturnStatus);
            _mockDatabaseCommand.Setup(cmd => cmd.AddParameter(It.IsAny<string>(), It.IsAny<object>()));
            _mockDatabaseCommand.SetupProperty(cmd => cmd.CommandText, MovieSeriesQueries.InsertSeries);
            _mockDatabaseConnection.Setup(db => db.CreateCommand()).Returns(_mockDatabaseCommand.Object);

            // Act
            var actualReturnStatus = _repository.AddMovieSeries(movieSeries);

            // Assert
            Assert.AreEqual(expectedReturnStatus, actualReturnStatus);
            _loggerVerifier.VerifyInformationMessage("HorrorTracker database is open.");
            _loggerVerifier.VerifyInformationMessage($"Movie series {movieSeries.Title} was added successfully.");
            _loggerVerifier.VerifyInformationMessage("HorrorTracker database is closed.");
        }

        [TestMethod]
        public void AddMovieSeries_SuccessfulConnectionAndDBNullResult_ShouldReturnNull()
        {
            // Arrange
            var expectedReturnStatus = DBNull.Value;
            var fixture = new Fixture();
            var movieSeries = fixture.Create<MovieSeries>();
            _mockDatabaseConnection.Setup(db => db.Open());
            _mockDatabaseCommand.Setup(cmd => cmd.ExecuteScalar()).Returns(expectedReturnStatus);
            _mockDatabaseCommand.Setup(cmd => cmd.AddParameter(It.IsAny<string>(), It.IsAny<object>()));
            _mockDatabaseCommand.SetupProperty(cmd => cmd.CommandText, MovieSeriesQueries.InsertSeries);
            _mockDatabaseConnection.Setup(db => db.CreateCommand()).Returns(_mockDatabaseCommand.Object);

            // Act
            var actualReturnStatus = _repository.AddMovieSeries(movieSeries);

            // Assert
            Assert.AreEqual(expectedReturnStatus, actualReturnStatus);
            _loggerVerifier.VerifyInformationMessage("HorrorTracker database is open.");
            _loggerVerifier.VerifyInformationMessage("HorrorTracker database is closed.");
        }

        [TestMethod]
        public void AddMovieSeries_SuccessfulConnectionAndNullResult_ShouldReturnNull()
        {
            // Arrange
            var fixture = new Fixture();
            var movieSeries = fixture.Create<MovieSeries>();
            _mockDatabaseConnection.Setup(db => db.Open());
            _mockDatabaseCommand.Setup(cmd => cmd.ExecuteScalar()).Returns((object?)null);
            _mockDatabaseCommand.Setup(cmd => cmd.AddParameter(It.IsAny<string>(), It.IsAny<object>()));
            _mockDatabaseCommand.SetupProperty(cmd => cmd.CommandText, MovieSeriesQueries.InsertSeries);
            _mockDatabaseConnection.Setup(db => db.CreateCommand()).Returns(_mockDatabaseCommand.Object);

            // Act
            var actualReturnStatus = _repository.AddMovieSeries(movieSeries);

            // Assert
            Assert.IsNull(actualReturnStatus);
            _loggerVerifier.VerifyInformationMessage("HorrorTracker database is open.");
            _loggerVerifier.VerifyInformationMessage("HorrorTracker database is closed.");
        }

        [TestMethod]
        public void AddMovieSeries_WhenExceptionOccurs_ShouldLogMessageAndReturnNull()
        {
            // Arrange
            var fixture = new Fixture();
            var movieSeries = fixture.Create<MovieSeries>();
            var exceptionMessage = "Failed for not able to connect to the server.";
            _mockDatabaseConnection.Setup(db => db.Open()).Throws(new Exception(exceptionMessage));
            _mockLoggerService.Setup(x => x.LogError(It.IsAny<string>(), It.IsAny<Exception>()));

            // Act
            var returnStatus = _repository.AddMovieSeries(movieSeries);

            // Assert
            Assert.IsNull(returnStatus);
            _loggerVerifier.VerifyErrorMessage("Adding a movie series to the database failed.", exceptionMessage);
            _loggerVerifier.VerifyInformationMessage("HorrorTracker database is closed.");
        }

        [TestMethod]
        public void GetMovieSeriesByName_ReturnsMovieSeries_WhenSeriesExists()
        {
            // Arrange
            var seriesName = "Test Series";
            var mockCommand = new Mock<IDatabaseCommand>();
            var mockDataReader = new Mock<IDataReader>();

            _mockDatabaseConnection.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);
            mockCommand.Setup(cmd => cmd.ExecuteReader()).Returns(mockDataReader.Object);

            mockDataReader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);

            mockDataReader.Setup(r => r.GetInt32(It.Is<int>(i => i == 0))).Returns(1);
            mockDataReader.Setup(r => r.GetString(It.Is<int>(i => i == 1))).Returns(seriesName);
            mockDataReader.Setup(r => r.GetDecimal(It.Is<int>(i => i == 2))).Returns(400);
            mockDataReader.Setup(r => r.GetInt32(It.Is<int>(i => i == 3))).Returns(11);
            mockDataReader.Setup(r => r.GetBoolean(It.Is<int>(i => i == 4))).Returns(false);

            // Act
            var returnedMovieSeries = _repository.GetMovieSeriesByName(seriesName);

            // Assert
            Assert.IsNotNull(returnedMovieSeries);
            Assert.AreEqual(1, returnedMovieSeries.Id);
            Assert.AreEqual(seriesName, returnedMovieSeries.Title);
            Assert.AreEqual(400, returnedMovieSeries.TotalTime);
            Assert.AreEqual(11, returnedMovieSeries.TotalMovies);
            Assert.IsFalse(returnedMovieSeries.Watched);
            _loggerVerifier.VerifyInformationMessage("HorrorTracker database is open.");
            _loggerVerifier.VerifyInformationMessage($"Movie series {seriesName} was found in the database.");
            _loggerVerifier.VerifyInformationMessage("HorrorTracker database is closed.");
        }

        [TestMethod]
        public void GetMovieSeriesByName_ReturnsNull_WhenSeriesDoesNotExist()
        {
            // Arrange
            var seriesName = "Nonexistent Series";
            var mockCommand = new Mock<IDatabaseCommand>();
            var mockDataReader = new Mock<IDataReader>();

            _mockDatabaseConnection.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);
            mockCommand.Setup(cmd => cmd.ExecuteReader()).Returns(mockDataReader.Object);
            mockDataReader.Setup(r => r.Read()).Returns(false);

            // Act
            var returnedMovieSeries = _repository.GetMovieSeriesByName(seriesName);

            // Assert
            Assert.IsNull(returnedMovieSeries);
            _loggerVerifier.VerifyWarningMessage($"Movie series {seriesName} was not found in the database.");
            _loggerVerifier.VerifyInformationMessage("HorrorTracker database is closed.");
        }

        [TestMethod]
        public void GetMovieSeriesByName_WhenExceptionOccurs_ShouldLogMessageAndReturnNull()
        {
            // Arrange
            var exceptionMessage = "Failed for not able to connect to the server.";
            _mockDatabaseConnection.Setup(db => db.Open()).Throws(new Exception(exceptionMessage));
            _mockLoggerService.Setup(x => x.LogError(It.IsAny<string>(), It.IsAny<Exception>()));

            // Act
            var returnStatus = _repository.GetMovieSeriesByName("movieSeries");

            // Assert
            Assert.IsNull(returnStatus);
            _loggerVerifier.VerifyErrorMessage("An error occurred while getting the movie series by name.", exceptionMessage);
            _loggerVerifier.VerifyInformationMessage("HorrorTracker database is closed.");
        }
    }
}