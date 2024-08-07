﻿using System.Diagnostics.CodeAnalysis;

namespace HorrorTracker.MSTests.Shared
{
    [ExcludeFromCodeCoverage]
    public static class Messages
    {
        /// <summary>
        /// HorrorTracker database is open.
        /// </summary>
        public const string DatabaseOpened = "HorrorTracker database is open.";

        /// <summary>
        /// HorrorTracker database is closed.
        /// </summary>
        public const string DatabaseClosed = "HorrorTracker database is closed.";

        /// <summary>
        /// Exception message.
        /// </summary>
        public const string ExceptionMessage = "Failed for not able to connect to the server.";
    }
}
