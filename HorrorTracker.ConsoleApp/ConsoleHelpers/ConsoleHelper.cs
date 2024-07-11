﻿using HorrorTracker.Utilities.Logging.Interfaces;
using HorrorTracker.Utilities.Parsing;
using System.Diagnostics.CodeAnalysis;

namespace HorrorTracker.ConsoleApp.ConsoleHelpers
{
    /// <summary>
    /// The <see cref="ConsoleHelper"/> class.
    /// </summary>
    public static class ConsoleHelper
    {
        /// <summary>
        /// Loading animation.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public static void ThinkingAnimation(string initialText, int numberOfDots, string doneText)
        {
            Console.Write(initialText);

            for (int i = 0; i < numberOfDots; i++)
            {
                Console.Write(".");
                Thread.Sleep(500);
            }

            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop);

            Console.WriteLine(doneText);
        }

        /// <summary>
        /// Types the message out.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void TypeMessage(string message)
        {
            foreach (var character in message)
            {
                Console.Write(character);
                Thread.Sleep(25);
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Processes the main decision based on user input.
        /// </summary>
        /// <param name="decision">The decision.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="actions">The dictionary of actions.</param>
        public static void ProcessDecision(string? decision, ILoggerService logger, Dictionary<int, Action> actions)
        {
            logger.LogInformation($"User input: {decision}");
            var parser = new Parser();
            _ = parser.IsInteger(decision, out var actualNumber);

            logger.LogInformation($"Processing decision: {actualNumber}");
            PerformActionsBasedOnDecision(actualNumber, logger, actions);
        }

        /// <summary>
        /// Performs the actions based on the user selection.
        /// </summary>
        /// <param name="actualNumber">The decision number.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="actions">The dictionary of actions.</param>
        private static void PerformActionsBasedOnDecision(int actualNumber, ILoggerService logger, Dictionary<int, Action> actions)
        {
            try
            {
                if (actions.TryGetValue(actualNumber, out Action? value))
                {
                    value();
                }
                else
                {
                    logger.LogWarning("Invalid selection made.");
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("Invalid selection. Please enter a valid number.");
                    Console.WriteLine();
                    Console.ResetColor();
                    Thread.Sleep(3000);
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Error processing decision.", ex);
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("An error occurred while processing your selection. Please try again.");
                Console.WriteLine();
                Console.ResetColor();
                Thread.Sleep(3000);
            }
        }
    }
}