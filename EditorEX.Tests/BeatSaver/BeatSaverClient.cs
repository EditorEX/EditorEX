using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EditorEX.Tests.BeatSaver
{
    public sealed class BeatSaverClient
    {
        public const string SkipEnvironmentVariable = "EDITOR_EX_SKIP_BEATSAVER";

        private static readonly HttpClient Http = CreateClient();

        private static readonly object CacheLock = new();

        public string CacheRoot { get; }

        public BeatSaverClient(string? cacheRoot = null)
        {
            CacheRoot =
                cacheRoot
                ?? Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".cache", "beatsaver");
        }

        public string GetExtractedPath(string hash)
        {
            return Path.GetFullPath(Path.Combine(CacheRoot, hash));
        }

        public bool IsCached(string hash)
        {
            string path = GetExtractedPath(hash);
            return Directory.Exists(path)
                && (
                    File.Exists(Path.Combine(path, "Info.dat"))
                    || File.Exists(Path.Combine(path, "info.dat"))
                );
        }

        public bool ShouldSkipDownload()
        {
            string? value = Environment.GetEnvironmentVariable(SkipEnvironmentVariable);
            return !string.IsNullOrEmpty(value)
                && !string.Equals(value, "0", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(value, "false", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<string> EnsureExtractedAsync(MapFixture fixture)
        {
            string extracted = GetExtractedPath(fixture.Hash);
            lock (CacheLock)
            {
                if (IsCached(fixture.Hash))
                {
                    return extracted;
                }
            }

            if (ShouldSkipDownload())
            {
                throw new BeatSaverSkippedException(
                    $"BeatSaver download skipped ({SkipEnvironmentVariable} is set) and cache is empty for {fixture.Hash}."
                );
            }

            Directory.CreateDirectory(CacheRoot);
            string zipPath = Path.Combine(CacheRoot, fixture.Hash + ".zip");
            string downloadUrl = await ResolveDownloadUrlAsync(fixture.Hash)
                .ConfigureAwait(false);

            lock (CacheLock)
            {
                if (IsCached(fixture.Hash))
                {
                    return extracted;
                }
            }

            using (HttpResponseMessage response = await Http.GetAsync(downloadUrl).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                using (
                    Stream source = await response.Content.ReadAsStreamAsync().ConfigureAwait(false)
                )
                using (FileStream dest = File.Create(zipPath))
                {
                    await source.CopyToAsync(dest).ConfigureAwait(false);
                }
            }

            lock (CacheLock)
            {
                if (IsCached(fixture.Hash))
                {
                    return extracted;
                }

                if (Directory.Exists(extracted))
                {
                    Directory.Delete(extracted, true);
                }

                Directory.CreateDirectory(extracted);
                ExtractZip(zipPath, extracted);
                return extracted;
            }
        }

        private static async Task<string> ResolveDownloadUrlAsync(string hash)
        {
            string metadataUrl = "https://api.beatsaver.com/maps/hash/" + hash;
            using (
                HttpResponseMessage response = await Http.GetAsync(metadataUrl).ConfigureAwait(false)
            )
            {
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                JObject root = JObject.Parse(json);
                JToken? versions = root["versions"];
                if (versions is JArray array)
                {
                    foreach (JToken version in array)
                    {
                        string? versionHash = version["hash"]?.ToString();
                        string? downloadUrl = version["downloadURL"]?.ToString();
                        if (
                            string.Equals(versionHash, hash, StringComparison.OrdinalIgnoreCase)
                            && !string.IsNullOrEmpty(downloadUrl)
                        )
                        {
                            return downloadUrl!;
                        }
                    }
                }

                string? first = versions?.First?["downloadURL"]?.ToString();
                if (!string.IsNullOrEmpty(first))
                {
                    return first!;
                }
            }

            return "https://cdn.beatsaver.com/" + hash + ".zip";
        }

        private static void ExtractZip(string zipPath, string destination)
        {
            using (FileStream stream = File.OpenRead(zipPath))
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        continue;
                    }

                    string relative = entry.FullName.Replace('/', Path.DirectorySeparatorChar);
                    string target = Path.Combine(destination, relative);
                    string? directory = Path.GetDirectoryName(target);
                    if (!string.IsNullOrEmpty(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    using (Stream source = entry.Open())
                    using (FileStream dest = File.Create(target))
                    {
                        source.CopyTo(dest);
                    }
                }
            }
        }

        private static HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("EditorEX.Tests");
            client.Timeout = TimeSpan.FromMinutes(2);
            return client;
        }
    }

    public sealed class BeatSaverSkippedException : Exception
    {
        public BeatSaverSkippedException(string message)
            : base(message) { }
    }
}
