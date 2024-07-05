﻿using HorrorTracker.Data.Models;

namespace HorrorTracker.Data.Repositories.Interfaces
{
    /// <summary>
    /// The <see cref="IMovieSeriesRepository"/> interface.
    /// </summary>
    public interface IMovieSeriesRepository
    {
        /// <summary>
        /// Adds a movie series to the database.
        /// </summary>
        /// <param name="series">The movie series.</param>
        /// <returns>The status.</returns>
        int AddMovieSeries(MovieSeries series);

        /// <summary>
        /// Retrieves the movie series by name.
        /// </summary>
        /// <param name="seriesName">The series name.</param>
        /// <returns>The movie series.</returns>
        MovieSeries? GetMovieSeriesByName(string seriesName);

        /// <summary>
        /// Updates the movie series.
        /// </summary>
        /// <param name="series">The movie series.</param>
        /// <returns>Message status.</returns>
        string UpdateSeries(MovieSeries series);

        /// <summary>
        /// Deletes the movie series.
        /// </summary>
        /// <param name="id">The movie series id.</param>
        /// <returns>Message status.</returns>
        string DeleteSeries(int id);

        /// <summary>
        /// Gets the unwatched or watched movies in a given series.
        /// </summary>
        /// <param name="seriesName">The series name.</param>
        /// <param name="query">The query.</param>
        /// <returns>The list of movies.</returns>
        IEnumerable<Movie> GetUnwatchedOrWatchedMoviesBySeriesName(string seriesName, string query);

        /// <summary>
        /// Updates the total time for a given movie series.
        /// </summary>
        /// <param name="seriesId">Teh series id.</param>
        /// <returns>Message status.</returns>
        string UpdateTotalTime(int seriesId);
    }
}