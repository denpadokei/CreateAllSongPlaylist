using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Security.Cryptography;

namespace CreateAllSongPlaylist
{
    internal static class PlaylistUtil
    {
        internal static async Task SaveAllSongs(string? bsInstallPath)
        {
            if (string.IsNullOrEmpty(bsInstallPath)) {
                return;
            }
            var customSongsDirectoryPath = Path.Combine(bsInstallPath, @"Beat Saber_Data", @"CustomLevels");
            var playlistDirectoryPath = Path.Combine(bsInstallPath, "Playlists");
            if (!Directory.Exists(customSongsDirectoryPath) || !Directory.Exists(playlistDirectoryPath)) {
                return;
            }
            var tasks = new List<Task<string>>();
            foreach (var item in Directory.EnumerateDirectories(customSongsDirectoryPath, "*", SearchOption.TopDirectoryOnly)) {
                tasks.Add(Task.Run(() => GenerateHash(item)));
            }
            var results = await Task.WhenAll(tasks);
            var playlistJson = new PlaylistEntity
            {
                playlistTitle = "All custom songs",
                playlistAuthor = Assembly.GetExecutingAssembly()?.GetName()?.Name ?? "",
                songs = new ReadOnlyCollection<SongInfoEntity>(results.Select(x => new SongInfoEntity
                {
                    hash = x,
                    levelid = $"custom_level_{x}",
                }).ToArray())
            };
            var jsonText = JsonConvert.SerializeObject(playlistJson, Formatting.Indented);
            var playlistPath = Path.Combine(playlistDirectoryPath, "AllCustomSongs.bplist");
            File.WriteAllText(playlistPath, jsonText);
        }

        /// <summary>
        /// Generates a hash for the song and assigns it to the SongHash field.
        /// Uses Kylemc1413's implementation from SongCore.
        /// TODO: Handle/document exceptions (such as if the files no longer exist when this is called).
        /// https://github.com/Kylemc1413/SongCore
        /// </summary>
        /// <returns>Hash of the song files. Null if the info.dat file doesn't exist</returns>
        private static string GenerateHash(string? songDirectory)
        {
            if (string.IsNullOrEmpty(songDirectory)) {
                return "";
            }
            var combinedBytes = Array.Empty<byte>();
            var infoFile = Path.Combine(songDirectory, "info.dat");
            if (!File.Exists(infoFile)) {
                return "";
            }

            combinedBytes = combinedBytes.Concat(File.ReadAllBytes(infoFile)).ToArray();
            var token = JToken.Parse(File.ReadAllText(infoFile));
            var beatMapSets = token["_difficultyBeatmapSets"];
            var numChars = beatMapSets?.Children().Count();
            for (var i = 0; i < numChars; i++) {
                var diffs = beatMapSets?.ElementAt(i);
                var numDiffs = diffs?["_difficultyBeatmaps"]?.Children().Count();
                for (var i2 = 0; i2 < numDiffs; i2++) {
                    var diff = diffs?["_difficultyBeatmaps"]?.ElementAt(i2);
                    var beatmapFileName = diff?["_beatmapFilename"]?.Value<string>();
                    if (string.IsNullOrEmpty(beatmapFileName)) {
                        continue;
                    }
                    var beatmapPath = Path.Combine(songDirectory, beatmapFileName);
                    if (File.Exists(beatmapPath)) {
                        combinedBytes = combinedBytes.Concat(File.ReadAllBytes(beatmapPath)).ToArray();
                    }
                }
            }

            var hash = CreateSha1FromBytes(combinedBytes.ToArray());
            return hash;
        }

        /// <summary>
        /// Returns the Sha1 hash of the provided byte array.
        /// Uses Kylemc1413's implementation from SongCore.
        /// https://github.com/Kylemc1413/SongCore
        /// </summary>
        /// <param name="input">Byte array to hash.</param>
        /// <returns>Sha1 hash of the byte array.</returns>
        private static string CreateSha1FromBytes(byte[] input)
        {
            using (var sha1 = SHA1.Create()) {
                var inputBytes = input;
                var hashBytes = sha1.ComputeHash(inputBytes);

                return BitConverter.ToString(hashBytes).Replace("-", string.Empty);
            }
        }
    }
}
