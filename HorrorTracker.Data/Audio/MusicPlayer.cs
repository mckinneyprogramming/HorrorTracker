﻿using HorrorTracker.Utilities.Logging;
using NAudio.Wave;
using System.Diagnostics.CodeAnalysis;

namespace HorrorTracker.Data.Audio
{
    /// <summary>
    /// The <see cref="MusicPlayer"/> class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class MusicPlayer
    {
        /// <summary>
        /// The folder containing the songs.
        /// </summary>
#pragma warning disable CS8604 // Possible null reference argument.
        private readonly string themeSongsFolder = Path.GetFullPath(Environment.GetEnvironmentVariable("HorrorThemeSongs"));
#pragma warning restore CS8604 // Possible null reference argument.

        /// <summary>
        /// The Random object.
        /// </summary>
        private readonly Random random;

        /// <summary>
        /// Queue of songs.
        /// </summary>
        private readonly Queue<string> songQueue;

        /// <summary>
        /// The logger service.
        /// </summary>
        private LoggerService _logger;

        /// <summary>
        /// The lock object.
        /// </summary>
        private readonly object lockObject = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="MusicPlayer"/> class.
        /// </summary>
        public MusicPlayer(LoggerService logger)
        {
            random = new Random();
            songQueue = new Queue<string>();
            _logger = logger;
        }
        
        /// <summary>
        /// Loads and shuffles the songs.
        /// </summary>
        public void LoadAndShuffleSongs()
        {
            _logger.LogInformation("Shuffling the music.");
            var themeSongs = Directory.GetFiles(themeSongsFolder, "*.mp3").ToList();
            Shuffle(themeSongs);
            foreach (var song in themeSongs)
            {
                songQueue.Enqueue(song);
            }
        }

        /// <summary>
        /// Plays the songs in the queue.
        /// </summary>
        public void StartPlaying()
        {
            Task.Run(() => PlaySongsInQueue());
        }

        /// <summary>
        /// Shuffles the songs in the folder.
        /// </summary>
        /// <param name="list">List of songs.</param>
        private void Shuffle(List<string> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = random.Next(n + 1);
                (list[n], list[k]) = (list[k], list[n]);
            }
        }

        /// <summary>
        /// Plays the songs in the queue.
        /// </summary>
        private void PlaySongsInQueue()
        {
            while (songQueue.Count > 0)
            {
                string songPath;
                lock (lockObject)
                {
                    songPath = songQueue.Dequeue();
                }

                PlaySong(songPath);
            }
        }

        /// <summary>
        /// Plays the individual song.
        /// </summary>
        /// <param name="filePath">The file path to the song.</param>
        private void PlaySong(string filePath)
        {
            _logger.LogInformation($"Play song {Path.GetFileNameWithoutExtension(filePath)}");
            using var audioFile = new AudioFileReader(filePath);
            using var outputDevice = new WaveOutEvent();
            outputDevice.Init(audioFile);
            outputDevice.Play();

            while (outputDevice.PlaybackState == PlaybackState.Playing)
            {
                Thread.Sleep(1000);
            }
        }
    }
}