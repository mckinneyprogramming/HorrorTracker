﻿using HorrorTracker.ConsoleApp.ConsoleHelpers;
using HorrorTracker.Data.Models;
using HorrorTracker.Data.Performers;
using HorrorTracker.Data.PostgreHelpers;
using HorrorTracker.Data.Repositories;
using HorrorTracker.Data.TMDB;
using HorrorTracker.Utilities.Logging;
using HorrorTracker.Utilities.Parsing;
using TMDbLib.Objects.Search;

namespace HorrorTracker.ConsoleApp.Managers
{
    /// <summary>
    /// The <see cref="MovieDatabaseApiManager"/> class.
    /// </summary>
    /// <seealso cref="Manager"/>
    public class MovieDatabaseApiManager : Manager
    {
        /// <summary>
        /// The connection string.
        /// </summary>
        private readonly string _connectionString;

        /// <summary>
        /// IsNotDone indicator.
        /// </summary>
        private bool IsNotDone = true;

        /// <summary>
        /// The parser.
        /// </summary>
        private readonly Parser _parser;

        /// <summary>
        /// Initializes a new instance of the <see cref="MovieDatabaseApiManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public MovieDatabaseApiManager(LoggerService logger, string connectionString)
            : base(logger)
        {
            _connectionString = connectionString;
            _parser = new Parser();
        }

        /// <inheritdoc/>
        public override void Manage()
        {
            while (IsNotDone)
            {
                DisplayManagerMenus();

                var decision = Console.ReadLine();
                var actions = MovieDatabaseApiDecisionActions();

                ConsoleHelper.ProcessDecision(decision, _logger, actions);
            }
        }

        /// <summary>
        /// Retrieves the dictionary of actions.
        /// </summary>
        /// <returns>The dictionary of actions.</returns>
        private Dictionary<int, Action> MovieDatabaseApiDecisionActions()
        {
            return new Dictionary<int, Action>()
            {
                { 1, SearchSeriesToAdd },
                { 2, SearchMovieToAdd },
                { 3, AddDocumentary },
                { 4, AddTelevisionShow },
                { 5, AddEpisode },
                { 6, FindSeriesToAdd },
                { 7, () => { IsNotDone = false; _logger.LogInformation("Selected to exit."); } }
            };
        }

        /// <summary>
        /// Adds a series and its movies to the database.
        /// </summary>
        private void SearchSeriesToAdd()
        {
            var decision = InitialUserDecision("----- Add Series to Datebase -----", "Search for a series below to add to the database.");
            if (_parser.StringIsNull(decision))
            {
                return;
            }

            var movieDatabaseService = CreateMovieDatabaseService();
            var result = movieDatabaseService.SearchCollection($"{decision} Collection").Result;

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            ConsoleHelper.TypeMessage("The following series were found based on your input:");
            Console.ForegroundColor = ConsoleColor.Magenta;
            foreach (var collection in result.Results)
            {
                Console.WriteLine($"- {collection.Name}; Id: {collection.Id}\n" +
                    $"  - {collection.Overview}");
            }

            var collectionId = PromptForSeriesId();
            if (collectionId == 0)
            {
                return;
            }

            var collectionInformation = movieDatabaseService.GetCollection(collectionId).Result;
            var filmsInSeries = FetchFilmsInSeries(movieDatabaseService, collectionInformation.Parts);
            var totalTimeOfFims = Convert.ToDecimal(filmsInSeries.Sum(film => film.Runtime));
            var seriesName = collectionInformation.Name.Replace("Collection", string.Empty).Trim();
            var numberOfFilms = collectionInformation.Parts.Count(part => part.ReleaseDate != null);
            var series = new MovieSeries(seriesName, totalTimeOfFims, numberOfFilms, false);

            var databaseConnection = new DatabaseConnection(_connectionString);
            var movieSeriesRepository = new MovieSeriesRepository(databaseConnection, _logger);

            if (!Inserter.MovieSeriesAddedSuccessfully(movieSeriesRepository, series))
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("The movie series you are trying to add alreday exists in the database or an error occurred. Please try a different series.");
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"The movie series '{series.Title}' was added successfully.");
            Thread.Sleep(1000);
            Console.ResetColor();

            var newSeries = movieSeriesRepository.GetMovieSeriesByName(series.Title);
            if (newSeries == null)
            {
                return;
            }

            foreach (var film in filmsInSeries)
            {
                var movie = new Movie(film.Title, Convert.ToDecimal(film.Runtime), true, newSeries.Id, film.ReleaseDate!.Value.Year, false);
                var movieRepository = new MovieRepository(databaseConnection, _logger);

                if (!Inserter.MovieAddedSuccessfully(movieRepository, movie))
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("The movie was not added. An error occurred or the movie was invalid.");
                    return;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"The movie '{film.Title}' was added successfully.");
                Thread.Sleep(1000);
                Console.ResetColor();
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Movie series: {series.Title} was added successfully as well as the movies in the series.");
            Thread.Sleep(2000);
            Console.ResetColor();
        }

        /// <summary>
        /// Adds a movie to the database.
        /// </summary>
        private void SearchMovieToAdd()
        {
            var decision = InitialUserDecision("----- Add Movie to Datebase -----", "Search for a movie below to add to the database.");
            if (_parser.StringIsNull(decision))
            {
                return;
            }

            var movieDatabaseService = CreateMovieDatabaseService();
        }

        /// <summary>
        /// Adds a documentary to the database.
        /// </summary>
        private void AddDocumentary()
        {
            Console.Clear();
        }

        /// <summary>
        /// Adds a television show to the database.
        /// </summary>
        private void AddTelevisionShow()
        {
            Console.Clear();
        }

        /// <summary>
        /// Adds a episode to the database.
        /// </summary>
        private void AddEpisode()
        {
            Console.Clear();
        }

        /// <summary>
        /// Finds a series to add to the database.
        /// </summary>
        private void FindSeriesToAdd()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("----- Find Collections to Add -----");
            Console.WriteLine();
            Console.ResetColor();
            Console.WriteLine("Select a Genre Id:");
            Console.WriteLine(
                "Horror - 27\n" +
                "Thriller - 53\n" +
                "Mystery - 9648");
            Console.Write(">> ");
            var genreId = Console.ReadLine();
            if (!_parser.IsInteger(genreId, out var genreInt))
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("The selection was not an integer. Please try again.");
                return;
            }

            var movieDatabaseService = CreateMovieDatabaseService();
            var totalPages = movieDatabaseService.GetNumberOfPages(genreInt).Result;

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"There are {totalPages} pages of films for the selected genre in TMDB API.");
            Console.ResetColor();
            ConsoleHelper.TypeMessage("Provide the number of pages you would like to search to find collections. We recommand no more than 400.");
            Console.Write("Start: ");
            var startPage = Console.ReadLine();
            Console.Write("End: ");
            var endPage = Console.ReadLine();

            var parser = new Parser();
            var startPageNotValid = !parser.IsInteger(startPage, out var startInt);
            var endPageNotValid = !parser.IsInteger(endPage, out var endInt);
            if (startPageNotValid || endPageNotValid)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("The start or end page was not a valid number.");
                return;
            }

            if (startInt > endInt || endInt > totalPages)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("The start page is greater than the end page or the end page is greater than the last page number.");
                return;
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Please stand by.");
            var collectionsFromCall = movieDatabaseService.GetHorrorCollections(startInt, endInt, genreInt).Result;
            Console.WriteLine("The following film series were found:");

            Console.ForegroundColor = ConsoleColor.Magenta;
            foreach (var series in collectionsFromCall)
            {
                Console.WriteLine($"- {series.Name}; Id: {series.Id}");
            }

            var collectionIds = PromptForSeriesIds();
            if (collectionIds.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("You did not provide a valid integer. Please try again.");
                Thread.Sleep(1000);
                Console.Clear();
                return;
            }

            foreach (var collectionId in collectionIds)
            {
                var collectionInformation = movieDatabaseService.GetCollection(collectionId).Result;
                var collectionName = collectionInformation.Name.Replace("Collection", "").Trim();
                var databaseConnection = new DatabaseConnection(_connectionString);
                var movieSeriesRepository = new MovieSeriesRepository(databaseConnection, _logger);
                var seriesExists = movieSeriesRepository.GetMovieSeriesByName(collectionName);
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
                    return;
                }

                var newSeries = new MovieSeries(collectionName, Convert.ToDecimal(filmsInSeries.Sum(s => s.Runtime)), filmsInSeries.Count, false);
                var addSeries = movieSeriesRepository.AddMovieSeries(newSeries);

                if (!Inserter.MovieSeriesAddedSuccessfully(movieSeriesRepository, newSeries))
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("The movie series you are trying to add alreday exists in the database or an error occurred. Please try a different series.");
                    return;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"The movie series '{newSeries.Title}' was added successfully.");
                Thread.Sleep(1000);
                Console.ResetColor();

                var addedSeries = movieSeriesRepository.GetMovieSeriesByName(newSeries.Title);
                if (addedSeries == null)
                {
                    return;
                }

                foreach (var film in filmsInSeries)
                {
                    var movie = new Movie(film.Title, Convert.ToDecimal(film.Runtime), true, addedSeries.Id, film.ReleaseDate!.Value.Year, false);
                    var movieRepository = new MovieRepository(databaseConnection, _logger);

                    if (!Inserter.MovieAddedSuccessfully(movieRepository, movie))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("The movie was not added. An error occurred or the movie was invalid.");
                        return;
                    }

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"The movie '{film.Title}' was added successfully.");
                    Thread.Sleep(1000);
                    Console.ResetColor();
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Movie series: {addedSeries.Title} was added successfully as well as the movies in the series.");
                Thread.Sleep(2000);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Retrieves the initial user decision to search the database.
        /// </summary>
        /// <param name="title">The console title.</param>
        /// <param name="prompt">The console prompt.</param>
        private static string? InitialUserDecision(string title, string prompt)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(title);
            Console.WriteLine();
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.DarkGray;
            ConsoleHelper.TypeMessage(prompt);
            Console.ResetColor();
            Console.WriteLine();
            Console.Write(">> ");
            return Console.ReadLine();
        }

        /// <summary>
        /// Creates TMDB API service.
        /// </summary>
        /// <returns>The movie database service.</returns>
        private static MovieDatabaseService CreateMovieDatabaseService()
        {
            var movieDatabaseConfig = new MovieDatabaseConfig();
            var movieDatabaseClientWrapper = new TMDbClientWrapper(movieDatabaseConfig.GetApiKey());
            return new MovieDatabaseService(movieDatabaseClientWrapper);
        }

        /// <summary>
        /// Collects the films in the series.
        /// </summary>
        /// <param name="movieDatabaseService">TMDB API service.</param>
        /// <param name="parts">The movies in the series.</param>
        /// <returns>The list of films.</returns>
        private static List<TMDbLib.Objects.Movies.Movie> FetchFilmsInSeries(MovieDatabaseService movieDatabaseService, List<SearchMovie> parts)
        {
            var filmsInSeries = new List<TMDbLib.Objects.Movies.Movie>();
            foreach (var part in parts.Where(part => part.ReleaseDate != null))
            {
                var film = movieDatabaseService.GetMovie(part.Id).Result;
                filmsInSeries.Add(film);
            }

            return filmsInSeries;
        }

        /// <summary>
        /// Retrieves the users input for the series id.
        /// </summary>
        /// <returns>The series id.</returns>
        private int PromptForSeriesId()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            ConsoleHelper.TypeMessage("Choose the series id above to add the series information to the database as well as its associated movies.");
            Console.ResetColor();
            Console.WriteLine();
            Console.Write(">> ");

            var collectionIdSelection = Console.ReadLine();
            if (_parser.IsInteger(collectionIdSelection, out var collectionId))
            {
                return collectionId;
            }

            return 0;
        }

        /// <summary>
        /// Retrieves the users list of series ids.
        /// </summary>
        /// <returns>List of integer series ids.</returns>
        private List<int> PromptForSeriesIds()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            ConsoleHelper.TypeMessage("Choose as many series ids above to add the series information to the database as well as its associated movies.");
            ConsoleHelper.TypeMessage("Separate the Ids by commas.");
            Console.ResetColor();
            Console.WriteLine();
            Console.Write(">> ");

            var idsSelection = Console.ReadLine();
            if (_parser.StringIsNull(idsSelection))
            {
                return [];
            }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var ids = idsSelection.Split(",");
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            var listOfIds = new List<int>();
            foreach (var id in ids)
            {
                if (_parser.IsInteger(id, out var integerId))
                {
                    listOfIds.Add(integerId);
                }
            }

            return listOfIds;
        }

        /// <inheritdoc/>
        protected override string RetrieveTitle() => "The Movie Database API";

        /// <inheritdoc/>
        protected override string RetrieveMenuOptions() => 
            "1. Search Series to Add\n" +
            "2. Add Movie\n" +
            "3. Add Documentary\n" +
            "4. Add TV Show\n" +
            "5. Add Episode\n" +
            "6. Find Series to Add\n" +
            "7. Exit";
    }
}