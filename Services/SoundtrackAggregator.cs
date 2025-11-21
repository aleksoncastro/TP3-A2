using System.Text.Json;
using MediaMatch.DTO.Soundtrack;

namespace MediaMatch.Services
{
    public class SoundtrackAggregator
    {
        private readonly TmdbService _tmdb;
        private readonly SpotifyService _spotify;
        private readonly AudioDbApiService _audioDb;

        public SoundtrackAggregator(TmdbService tmdb, SpotifyService spotify, AudioDbApiService audioDb)
        {
            _tmdb = tmdb;
            _spotify = spotify;
            _audioDb = audioDb;
        }

        public async Task<SoundtrackDto?> GetMovieSoundtrackAsync(int movieId)
        {
            var detailsJson = await _tmdb.GetMovieDetailsAsync(movieId);
            var creditsJson = await _tmdb.GetMovieCreditsAsync(movieId);

            var title = GetTitle(detailsJson);
            var year = GetYear(detailsJson);
            var composer = GetComposer(creditsJson);

            var album = await _spotify.SearchAlbumAsync(title, year, composer);
            if (album == null)
            {
                album = await _spotify.SearchAlbumAsync(title, year, null);
            }
            if (album == null)
            {
                album = await _spotify.SearchAlbumBasicAsync(title, year);
            }
            if (album == null)
            {
                album = await _spotify.SearchAlbumLooseAsync($"{title} soundtrack");
            }
            if (album == null) return null;

            var tracks = await _spotify.GetAlbumTracksAsync(album.id);

            var result = new SoundtrackDto
            {
                source = "spotify",
                composer = composer,
                album = new SoundtrackAlbumDto
                {
                    id = album.id,
                    name = album.name,
                    url = album.external_urls?.spotify ?? string.Empty,
                    release_date = album.release_date,
                    artists = album.artists?.Select(a => a.name).ToList() ?? new List<string>()
                },
                tracks = new List<SoundtrackTrackDto>(),
                confidence = ComputeConfidence(album.name, title)
            };

            foreach (var t in tracks)
            {
                var artists = t.artists?.Select(a => a.name).ToList() ?? new List<string>();
                var enriched = await _audioDb.SearchTrackAsync(artists.FirstOrDefault() ?? string.Empty, t.name);
                var dto = enriched?.track?.FirstOrDefault();
                if (dto == null)
                {
                    var byName = await _audioDb.SearchTrackByNameAsync(t.name);
                    dto = byName?.track?.FirstOrDefault();
                }

                var thumb = dto?.strTrackThumb ?? string.Empty;
                var video = dto?.strMusicVid ?? string.Empty;
                var mood = dto?.strMood ?? string.Empty;
                var description = dto?.strDescriptionEN ?? string.Empty;
                var genre = dto?.strGenre ?? string.Empty;

                if (string.IsNullOrEmpty(genre) && !string.IsNullOrEmpty(dto?.idAlbum))
                {
                    if (int.TryParse(dto.idAlbum, out var aid))
                    {
                        var albumRes = await _audioDb.GetAlbumByIdAsync(aid);
                        var albumDto = albumRes?.album?.FirstOrDefault();
                        genre = albumDto?.strGenre ?? string.Empty;
                    }
                }

                result.tracks.Add(new SoundtrackTrackDto
                {
                    id = t.id,
                    title = t.name,
                    artists = artists,
                    duration_ms = t.duration_ms,
                    url = t.external_urls?.spotify ?? string.Empty,
                    preview_url = t.preview_url,
                    genre = genre,
                    mood = mood,
                    description = description,
                    thumb_url = thumb,
                    video_url = video
                });
            }

            return result;
        }

        public async Task<SoundtrackDto?> GetSeriesSoundtrackAsync(int seriesId)
        {
            var detailsJson = await _tmdb.GetSeriesDetailsAsync(seriesId);
            var creditsJson = await _tmdb.GetSeriesCreditsAsync(seriesId);

            var title = GetTitle(detailsJson);
            var year = GetYear(detailsJson);
            var composer = GetComposer(creditsJson);

            var album = await _spotify.SearchAlbumAsync(title, year, composer);
            if (album == null)
            {
                album = await _spotify.SearchAlbumAsync(title, year, null);
            }
            if (album == null)
            {
                album = await _spotify.SearchAlbumBasicAsync(title, year);
            }
            if (album == null)
            {
                album = await _spotify.SearchAlbumLooseAsync($"{title} soundtrack");
            }
            if (album == null) return null;

            var tracks = await _spotify.GetAlbumTracksAsync(album.id);

            var result = new SoundtrackDto
            {
                source = "spotify",
                composer = composer,
                album = new SoundtrackAlbumDto
                {
                    id = album.id,
                    name = album.name,
                    url = album.external_urls?.spotify ?? string.Empty,
                    release_date = album.release_date,
                    artists = album.artists?.Select(a => a.name).ToList() ?? new List<string>()
                },
                tracks = new List<SoundtrackTrackDto>(),
                confidence = ComputeConfidence(album.name, title)
            };

            foreach (var t in tracks)
            {
                var artists = t.artists?.Select(a => a.name).ToList() ?? new List<string>();
                var enriched = await _audioDb.SearchTrackAsync(artists.FirstOrDefault() ?? string.Empty, t.name);
                var dto = enriched?.track?.FirstOrDefault();

                result.tracks.Add(new SoundtrackTrackDto
                {
                    id = t.id,
                    title = t.name,
                    artists = artists,
                    duration_ms = t.duration_ms,
                    url = t.external_urls?.spotify ?? string.Empty,
                    preview_url = t.preview_url,
                    genre = string.Empty,
                    mood = string.Empty,
                    description = string.Empty,
                    thumb_url = dto?.strTrackThumb ?? string.Empty,
                    video_url = string.Empty
                });
            }

            return result;
        }

        private static string GetTitle(string json)
        {
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("original_title", out var ot)) return ot.GetString() ?? string.Empty;
            if (doc.RootElement.TryGetProperty("original_name", out var on)) return on.GetString() ?? string.Empty;
            if (doc.RootElement.TryGetProperty("title", out var t)) return t.GetString() ?? string.Empty;
            if (doc.RootElement.TryGetProperty("name", out var n)) return n.GetString() ?? string.Empty;
            return string.Empty;
        }

        private static int? GetYear(string json)
        {
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("release_date", out var rd))
            {
                var s = rd.GetString();
                if (!string.IsNullOrWhiteSpace(s) && s.Length >= 4 && int.TryParse(s.Substring(0, 4), out var y)) return y;
            }
            if (doc.RootElement.TryGetProperty("first_air_date", out var fad))
            {
                var s = fad.GetString();
                if (!string.IsNullOrWhiteSpace(s) && s.Length >= 4 && int.TryParse(s.Substring(0, 4), out var y)) return y;
            }
            return null;
        }

        private static string GetComposer(string creditsJson)
        {
            var doc = JsonDocument.Parse(creditsJson);
            if (doc.RootElement.TryGetProperty("crew", out var crew))
            {
                foreach (var c in crew.EnumerateArray())
                {
                    if (c.TryGetProperty("job", out var job))
                    {
                        var j = job.GetString() ?? string.Empty;
                        if (string.Equals(j, "Original Music Composer", StringComparison.OrdinalIgnoreCase) || string.Equals(j, "Composer", StringComparison.OrdinalIgnoreCase))
                        {
                            if (c.TryGetProperty("name", out var name)) return name.GetString() ?? string.Empty;
                        }
                    }
                }
            }
            return string.Empty;
        }

        private static string ComputeConfidence(string albumName, string title)
        {
            var a = albumName.ToLowerInvariant();
            var t = title.ToLowerInvariant();
            var score = 0;
            if (a.Contains(t)) score += 5;
            if (a.Contains("original motion picture soundtrack")) score += 4;
            if (a.Contains("original score")) score += 3;
            if (a.Contains("soundtrack")) score += 2;
            if (score >= 8) return "high";
            if (score >= 5) return "medium";
            return "low";
        }
    }
}