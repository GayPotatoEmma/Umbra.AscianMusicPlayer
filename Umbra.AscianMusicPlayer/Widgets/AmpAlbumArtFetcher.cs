using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Umbra.AscianMusicPlayer.Widgets;

internal static class AmpAlbumArtFetcher
{
    private static readonly HttpClient Http = new();

    // null value means "fetched but no result found"
    private static readonly ConcurrentDictionary<string, byte[]?> Cache  = new();
    private static readonly ConcurrentDictionary<string, bool>    InFlight = new();

    public static byte[]? GetCached(string key) =>
        Cache.TryGetValue(key, out var bytes) ? bytes : null;

    public static void Request(string title, string artist, string album)
    {
        if (string.IsNullOrEmpty(title)) return;

        string key = $"{artist}|{title}|{album}";
        if (Cache.ContainsKey(key) || !InFlight.TryAdd(key, true)) return;

        Task.Run(async () => {
            try {
                Cache[key] = await FetchArtUrl(artist, title, album)
                          ?? await FetchArtUrl(artist, title, null)
                          ?? await FetchArtUrl(null,   title, null);
            } catch {
                Cache[key] = null;
            } finally {
                InFlight.TryRemove(key, out _);
            }
        });
    }
    private static async Task<byte[]?> FetchArtUrl(string? artist, string title, string? album)
    {
        var parts = new[] { artist, title, album }.Where(s => !string.IsNullOrEmpty(s));
        string term = Uri.EscapeDataString(string.Join(" ", parts));
        string url  = $"https://itunes.apple.com/search?term={term}&media=music&entity=musicTrack&limit=1";
        string json = await Http.GetStringAsync(url);

        using var doc  = JsonDocument.Parse(json);
        var       root = doc.RootElement;

        if (root.GetProperty("resultCount").GetInt32() == 0) return null;

        string artUrl = root.GetProperty("results")[0]
            .GetProperty("artworkUrl100")
            .GetString()!
            .Replace("100x100bb", "600x600bb");

        return await Http.GetByteArrayAsync(artUrl);
    }
}
