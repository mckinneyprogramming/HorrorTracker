﻿using HorrorTracker.Data.TMDB.Interfaces;
using TMDbLib.Objects.Collections;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Lists;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.Search;
using TMDbLib.Objects.TvShows;

namespace HorrorTracker.Data.TMDB
{
    /// <summary>
    /// The <see cref="MovieDatabaseService"/>
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="MovieDatabaseService"/> class.
    /// </remarks>
    public class MovieDatabaseService(ITMDbClientWrapper client)
    {
        /// <summary>
        /// The TMDB client.
        /// </summary>
        private readonly ITMDbClientWrapper _client = client;

        /// <summary>
        /// Retrieves a list of collections in TMDB API.
        /// </summary>
        /// <param name="seriesTitle">The series title.</param>
        /// <returns>The list of collections.</returns>
        public async Task<SearchContainer<SearchCollection>> SearchCollection(string seriesTitle)
        {
            return await _client.SearchCollectionAsync(seriesTitle);
        }

        /// <summary>
        /// Retrieves a list of movies in the TMDB API.
        /// </summary>
        /// <param name="movieName">The movie name.</param>
        /// <returns>The list of movies.</returns>
        public async Task<SearchContainer<SearchMovie>> SearchMovie(string movieName)
        {
            return await _client.SearchMovieAsync(movieName);
        }

        /// <summary>
        /// Retrieves a list of TV shows in the TMDB API.
        /// </summary>
        /// <param name="tvShow">The TV show.</param>
        /// <returns>The list of TV shows.</returns>
        public async Task<SearchContainer<SearchTv>> SearchTvShow(string tvShow)
        {
            return await _client.SearchTvShowAsync(tvShow);
        }

        /// <summary>
        /// Retrieves a collection in TMDB API.
        /// </summary>
        /// <param name="seriesId">The series id.</param>
        /// <returns>The collection.</returns>
        public async Task<Collection> GetCollection(int seriesId)
        {
            return await _client.GetCollectionAsync(seriesId);
        }

        /// <summary>
        /// Retrieves a movie in TMDB API.
        /// </summary>
        /// <param name="movieId">The movie id.</param>
        /// <returns>The movie.</returns>
        public async Task<Movie> GetMovie(int movieId)
        {
            return await _client.GetMovieAsync(movieId);
        }

        /// <summary>
        /// Retrieves the TV show in TMDB API.
        /// </summary>
        /// <param name="tvShowId">The tv show id.</param>
        /// <returns>The TV show.</returns>
        public async Task<TvShow> GetTvShow(int tvShowId)
        {
            return await _client.GetTvShowAsync(tvShowId);
        }

        /// <summary>
        /// Retrieves the TV season in TMDB API.
        /// </summary>
        /// <param name="tvShowId">The TV show id.</param>
        /// <param name="seasonNumber">The season number.</param>
        /// <returns>The TV season.</returns>
        public async Task<TvSeason> GetTvSeason(int tvShowId, int seasonNumber)
        {
            return await _client.GetTvSeasonAsync(tvShowId, seasonNumber);
        }

        /// <summary>
        /// Retrieves the TV episode in TMDB API.
        /// </summary>
        /// <param name="tvShowId">The TV show id.</param>
        /// <param name="seasonNumber">The season number.</param>
        /// <param name="episodeNumber">The episode number.</param>
        /// <returns>The TV episode.</returns>
        public async Task<TvEpisode> GetTvEpisode(int tvShowId, int seasonNumber, int episodeNumber)
        {
            return await _client.GetTvEpisodeAsync(tvShowId, seasonNumber, episodeNumber);
        }

        /// <summary>
        /// Retrieves the list of horror collections.
        /// </summary>
        /// <param name="startPage">The start page.</param>
        /// <param name="endPage">The end page.</param>
        /// <param name="genreId">The genre id.</param>
        /// <returns>The list of horror collections.</returns>
        public async Task<HashSet<SearchCollection>> GetHorrorCollections(int startPage, int endPage, int genreId)
        {
            return await _client.GetHorrorCollections(startPage, endPage, genreId);
        }

        /// <summary>
        /// Retrieves the number of pages in the horror genre of movies.
        /// </summary>
        /// <param name="genreId">The genre id.</param>
        /// <returns>The number of pages.</returns>
        public async Task<int> GetNumberOfPages(int genreId)
        {
            return await _client.GetNumberOfPages(genreId);
        }

        /// <summary>
        /// Retrieves the list of upcoming horror movies.
        /// </summary>
        /// <returns>The list of movies.</returns>
        public async Task<List<SearchMovie>> GetUpcomingHorrorMovies()
        {
            return await _client.GetUpcomingHorrorMoviesAsync();
        }

        /// <summary>
        /// Get lists from TMDB account.
        /// </summary>
        /// <returns>The lists from the TMBD account.</returns>
        public async Task<SearchContainer<AccountList>> GetLists()
        {
            return await _client.GetLists();
        }
    }
}