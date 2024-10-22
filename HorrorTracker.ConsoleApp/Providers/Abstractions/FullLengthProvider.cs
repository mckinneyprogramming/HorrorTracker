﻿using HorrorTracker.ConsoleApp.ConsoleHelpers;
using HorrorTracker.Data.Models;
using HorrorTracker.Data.Performers;
using HorrorTracker.Data.PostgreHelpers;
using HorrorTracker.Data.Repositories;
using HorrorTracker.Data.TMDB;
using HorrorTracker.Utilities.Logging;
using TMDbLib.Objects.Search;

namespace HorrorTracker.ConsoleApp.Providers.Abstractions
{
    /// <summary>
    /// The <see cref="FullLengthProvider"/> abstract class.
    /// </summary>
    /// <see cref="ProviderBase"/>
    public abstract class FullLengthProvider : ProviderBase
    {
        /// <summary>
        /// Adds the collections and the movies from TMBD to the database.
        /// </summary>
        /// <param name="movieDatabaseService">The movie database service.</param>
        /// <param name="collectionIds">The list of collection ids.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="logger">The logger service.</param>
        protected static void AddCollectionsAndMoviesToDatabase(
            MovieDatabaseService movieDatabaseService,
            List<int> collectionIds,
            string connectionString,
            LoggerService logger)
        {
            foreach (var collectionId in collectionIds)
            {
                var collectionInformation = movieDatabaseService.GetCollection(collectionId).Result;
                var collectionName = collectionInformation.Name.Replace("Collection", "").Trim();
                var databaseConnection = new DatabaseConnection(connectionString);
                var movieSeriesRepository = new MovieSeriesRepository(databaseConnection, logger);
                var seriesExists = movieSeriesRepository.GetByTitle(collectionName);
                if (seriesExists != null)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("The series you selected already exists in the database. Please try again.");
                    Thread.Sleep(1000);
                    Console.Clear();
                    return;
                }

                var filmsInSeries = FetchFilmsInSeries(movieDatabaseService, collectionInformation.Parts);
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"Series Name: {collectionInformation.Name}; Parts:");
                foreach (var film in filmsInSeries)
                {
                    Console.WriteLine($"- Title: {film.Title}; Runtime: {film.Runtime}, Release Year: {film.ReleaseDate!.Value.Year}\n" +
                        $"   - Overview {film.Overview}");
                }

                Console.ForegroundColor = ConsoleColor.DarkGray;
                ConsoleHelper.TypeMessage("Would you like to add this series and its movies to your database? Type Y or N");
                Console.ResetColor();
                Console.WriteLine();
                Console.Write(">> ");
                var addToDatabase = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(addToDatabase) || !addToDatabase.Equals("Y", StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }

                var newSeries = new MovieSeries(collectionName, Convert.ToDecimal(filmsInSeries.Sum(s => s.Runtime)), filmsInSeries.Count, false);
                if (!Inserter.MovieSeriesAddedSuccessfully(movieSeriesRepository, newSeries))
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("The movie series you are trying to add already exists in the database or an error occurred. Please try a different series.");
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"The movie series '{newSeries.Title}' was added successfully.");
                Thread.Sleep(1000);
                Console.ResetColor();

                var addedSeries = movieSeriesRepository.GetByTitle(newSeries.Title);
                if (addedSeries == null)
                {
                    continue;
                }

                AddFilmsToDatabase(databaseConnection, filmsInSeries, addedSeries.Id, logger);
                SeriesAddedToDatabaseMessages(addedSeries.Title);
            }
        }

        /// <summary>
        /// Collects the series and movie information and adds them to the database.
        /// </summary>
        /// <param name="movieDatabaseService">Movie database service.</param>
        /// <param name="collectionId">The collection id.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="logger">The logger.</param>
        protected static void AddSeriesAndMoviesToDatabase(MovieDatabaseService movieDatabaseService, int collectionId, string connectionString, LoggerService logger)
        {
            var collectionInformation = movieDatabaseService.GetCollection(collectionId).Result;
            var filmsInSeries = FetchFilmsInSeries(movieDatabaseService, collectionInformation.Parts);
            var totalTimeOfFims = Convert.ToDecimal(filmsInSeries.Sum(film => film.Runtime));
            var seriesName = collectionInformation.Name.Replace("Collection", string.Empty).Trim();
            var numberOfFilms = collectionInformation.Parts.Count(part => part.ReleaseDate != null);
            var series = new MovieSeries(seriesName, totalTimeOfFims, numberOfFilms, false);
            var databaseConnection = new DatabaseConnection(connectionString);
            var movieSeriesRepository = new MovieSeriesRepository(databaseConnection, logger);

            if (!Inserter.MovieSeriesAddedSuccessfully(movieSeriesRepository, series))
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("The movie series you are trying to add already exists in the database or an error occurred. Please try a different series.");
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"The movie series '{series.Title}' was added successfully.");
            Thread.Sleep(1000);
            Console.ResetColor();

            var newSeries = movieSeriesRepository.GetByTitle(series.Title);
            if (newSeries == null)
            {
                return;
            }

            AddFilmsToDatabase(databaseConnection, filmsInSeries, newSeries.Id, logger);
            SeriesAddedToDatabaseMessages(newSeries.Title);
        }

        /// <summary>
        /// Displays the message for the series being added to the database.
        /// </summary>
        /// <param name="title">The series title.</param>
        private static void SeriesAddedToDatabaseMessages(string title)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Movie series: {title} was added successfully as well as the movies in the series.");
            Thread.Sleep(2000);
            Console.ResetColor();
        }

        /// <summary>
        /// Adds the films from the collection to the database.
        /// </summary>
        /// <param name="databaseConnection">The database connection.</param>
        /// <param name="filmsInSeries">The films to add to the database.</param>
        /// <param name="addedSeriesId">The movie series id.</param>
        /// <param name="logger">The logger service.</param>
        private static void AddFilmsToDatabase(
            DatabaseConnection databaseConnection,
            List<TMDbLib.Objects.Movies.Movie> filmsInSeries,
            int addedSeriesId,
            LoggerService logger)
        {
            foreach (var film in filmsInSeries)
            {
                var movie = new Movie(film.Title, Convert.ToDecimal(film.Runtime), true, addedSeriesId, film.ReleaseDate!.Value.Year, false);
                var movieRepository = new MovieRepository(databaseConnection, logger);

                if (!Inserter.MovieAddedSuccessfully(movieRepository, movie))
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("The movie was not added. An error occurred or the movie was invalid.");
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"The movie '{film.Title}' was added successfully.");
                Thread.Sleep(1000);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Collects the films in the series.
        /// </summary>
        /// <param name="movieDatabaseService">TMDB API service.</param>
        /// <param name="parts">The movies in the series.</param>
        /// <returns>The list of films.</returns>
        protected static List<TMDbLib.Objects.Movies.Movie> FetchFilmsInSeries(MovieDatabaseService movieDatabaseService, List<SearchMovie> parts)
        {
            var filmsInSeries = new List<TMDbLib.Objects.Movies.Movie>();
            foreach (var part in parts.Where(part => part.ReleaseDate != null))
            {
                var film = movieDatabaseService.GetMovie(part.Id).Result;
                filmsInSeries.Add(film);
            }

            return filmsInSeries;
        }
    }
}