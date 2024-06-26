﻿using HorrorTracker.ConsoleApp.ConsoleHelpers;
using HorrorTracker.ConsoleApp.Managers;
using HorrorTracker.Data;
using HorrorTracker.Data.PostgreHelpers;
using HorrorTracker.Utilities.Logging;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace HorrorTracker.ConsoleApp
{
    /// <summary>
    /// The <see cref="Program"/> class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    static class Program
    {
        /// <summary>
        /// The connection string.
        /// </summary>
        private static readonly string _connectionString = ConfigurationManager.ConnectionStrings["HorrorTracker"].ConnectionString;

        /// <summary>
        /// IsNotDone indicator.
        /// </summary>
        private static bool IsNotDone = true;

        /// <summary>
        /// The logger service.
        /// </summary>
        private static readonly LoggerService _logger = new();

        /// <summary>
        /// The main method.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {
            _logger.LogInformation("HorrorTracker has started.");

            try
            {
                Console.Title = ConsoleTitles.RetrieveTitle("Home");

                TestDatabaseConnection();

                while (IsNotDone)
                {
                    Console.Clear();
                    DisplayMainMenu();
                    var decision = ConsoleHelper.GetUserInput();
                    var actualNumber = ConsoleHelper.ParseNumberDecision(_logger, decision);
                    var actions = MainMenuDecisionActions();

                    ConsoleHelper.ProcessDecision(actualNumber, _logger, actions);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("An unexpected error occurred.", ex);
            }
            finally
            {
                _logger.LogInformation("HorrorTracker has ended.");
                _logger.CloseAndFlush();
                ExitApplication();
            }
        }

        /// <summary>
        /// Tests the connection to the database.
        /// </summary>
        private static void TestDatabaseConnection()
        {
            _logger.LogInformation("Testing the Postgre database server and connection to the HorrorTracker database.");
            ConsoleHelper.GroupedConsole(ConsoleColor.DarkGray, "We are testing the connection to the database. Please standby.");
            ConsoleHelper.ThinkingAnimation("Testing", 10, "Testing Complete!");
            ConsoleHelper.NewLine();

            try
            {
                var databaseConnection = new DatabaseConnection(_connectionString);
                var connections = new HorrorConnections(databaseConnection, _logger);
                var connectionMessage = connections.Connect();

                if (connectionMessage.Contains("successful!"))
                {
                    _ = connections.CreateTables();
                    ConsoleHelper.GroupedConsole(ConsoleColor.Green, connectionMessage);
                    ConsoleHelper.ThinkingAnimation("Directing to Main Menu", 10, "Have Fun!");
                    Thread.Sleep(3000);
                }
                else
                {
                    ConsoleHelper.GroupedConsole(ConsoleColor.DarkRed, connectionMessage);
                    ConsoleHelper.ThinkingAnimation("Exiting Horror Tracker", 10, "Goodbye!");
                    Thread.Sleep(3000);

                    IsNotDone = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to connect to the database.", ex);
                ConsoleHelper.GroupedConsole(ConsoleColor.DarkRed, "Failed to connect to the database. Exiting...");
                IsNotDone = false;
            }
        }

        /// <summary>
        /// Displays the main menu.
        /// </summary>
        private static void DisplayMainMenu()
        {
            ConsoleHelper.GroupedConsole(ConsoleColor.Red, "========== Horror Tracker ==========");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            ConsoleHelper.TypeMessage("The Horror Tracker system uses TMDB (The Movie Database) API to quickly add items.");
            ConsoleHelper.TypeMessage("You will have the option below to add items manually or from TMDB API.");
            ConsoleHelper.NewLine();
            OverallSystemInformation();
            Thread.Sleep(2000);

            Console.ResetColor();
            Console.WriteLine(
                "1. Use TMDB API\n" +
                "2. Manually Add\n" +
                "3. Exit");
            Console.Write(">> ");

            _logger.LogInformation("Main menu displayed.");
        }

        /// <summary>
        /// Displays the overall information from the database.
        /// </summary>
        private static void OverallSystemInformation()
        {
            var databaseConnection = new DatabaseConnection(_connectionString);
            var horrorConnections = new HorrorConnections(databaseConnection, _logger);
            var overallRepository = horrorConnections.RetrieveOverallRepository();
            var overallTime = overallRepository.GetOverallTime();
            var overallTimeLeft = overallRepository.GetOverallTimeLeft();

            ConsoleHelper.GroupedConsole(ConsoleColor.Red, "===== Overall Information =====");
            Console.WriteLine($"Overall Time in the Database:");
            Console.WriteLine($"- In Hours: {overallTime}");
            Console.WriteLine($"- In Days: {overallTime / 24}");
            Console.WriteLine($"Overall Time left to Watch in the Database:");
            Console.WriteLine($"- In Hours: {overallTimeLeft}");
            Console.WriteLine($"- In Days: {overallTimeLeft / 24}");
            ConsoleHelper.NewLine();
        }

        /// <summary>
        /// Retrieves the main menu decision actions.
        /// </summary>
        /// <returns>The dictionary of actions.</returns>
        private static Dictionary<int, Action> MainMenuDecisionActions()
        {
            return new Dictionary<int, Action>
            {
                { 1, () => new MovieDatabaseApiManager(_logger, _connectionString).Manage() },
                { 2, () => new ManualManager(_logger).Manage() },
                { 3, () => { IsNotDone = false; _logger.LogInformation("Selected to exit."); } }
            };
        }

        /// <summary>
        /// Exits the application.
        /// </summary>
        private static void ExitApplication()
        {
            Console.ResetColor();
            Console.Write("Press any key to exit...");
            _ = Console.ReadKey();
        }
    }
}