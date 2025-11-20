using MediaMatch.Data;
using MediaMatch.Models.TADB;
using Microsoft.EntityFrameworkCore;

namespace MediaMatch.Services
{
    public class AudioDbImportService
    {
        private readonly MediaMatchContext _db;
        private readonly AudioDbApiService _api;

        public AudioDbImportService(MediaMatchContext db, AudioDbApiService api)
        {
            _db = db;
            _api = api;
        }

        // =======================================================
        // IMPORTAR ARTISTA
        // =======================================================
        public async Task<Artist?> ImportArtistAsync(string name)
        {
            var result = await _api.SearchArtistAsync(name);

            if (result?.artists == null || result.artists.Count == 0)
                return null;

            var dto = result.artists.First();

            var existing = await _db.Artists
                .FirstOrDefaultAsync(a => a.AudioDbArtistId == int.Parse(dto.idArtist));

            if (existing != null)
                return existing;

            var artist = new Artist
            {
                Name = dto.strArtist ?? "",
                Biography = dto.strBiographyEN ?? "",
                Country = dto.strCountry ?? "",
                AudioDbArtistId = int.Parse(dto.idArtist),
                ThumbnailUrl = dto.strArtistThumb
            };

            _db.Artists.Add(artist);
            await _db.SaveChangesAsync();

            return artist;
        }


        // =======================================================
        // IMPORTAR ÁLBUNS
        // =======================================================
        public async Task<List<Album>> ImportAlbumsAsync(int artistId)
        {
            var result = await _api.GetAlbumsAsync(artistId);

            if (result?.album == null || result.album.Count == 0)
                return new List<Album>();

            List<Album> imported = new();

            foreach (var dto in result.album)
            {
                int albumId = int.Parse(dto.idAlbum);

                var exists = await _db.Albums
                    .FirstOrDefaultAsync(a => a.AudioDbAlbumId == albumId);

                if (exists != null)
                {
                    imported.Add(exists);
                    continue;
                }

                int year = 0;
                int.TryParse(dto.intYearReleased, out year);

                var album = new Album
                {
                    Title = dto.strAlbum ?? "",
                    YearReleased = year,
                    Genre = dto.strGenre ?? "",
                    Label = dto.strLabel ?? "",
                    CoverUrl = dto.strAlbumThumb ?? "",
                    AudioDbAlbumId = albumId,
                    ArtistId = artistId
                };

                _db.Albums.Add(album);
                imported.Add(album);
            }

            await _db.SaveChangesAsync();
            return imported;
        }


        // =======================================================
        // IMPORTAR FAIXAS
        // =======================================================
        public async Task<List<Track>> ImportTracksAsync(int albumId)
        {
            var result = await _api.GetTracksAsync(albumId);

            if (result?.track == null || result.track.Count == 0)
                return new List<Track>();

            List<Track> imported = new();

            foreach (var dto in result.track)
            {
                int trackId = int.Parse(dto.idTrack);

                var exists = await _db.Tracks
                    .FirstOrDefaultAsync(t => t.AudioDbTrackId == trackId);

                if (exists != null)
                {
                    imported.Add(exists);
                    continue;
                }

                int duration = 0;
                int.TryParse(dto.intDuration, out duration);

                var track = new Track
                {
                    Title = dto.strTrack ?? "",
                    Duration = duration,
                    AudioDbTrackId = trackId,
                    PreviewUrl = dto.strTrackThumb ?? "",
                    AlbumId = albumId
                };

                _db.Tracks.Add(track);
                imported.Add(track);
            }

            await _db.SaveChangesAsync();
            return imported;
        }

    }
}
