
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaMatch.Models.TADB
{
    public class ArtistGenre
    {
        public int ArtistId { get; set; }
        [ForeignKey("ArtistId")]
        public Artist Artist { get; set; }

        public int MusicGenreId { get; set; }
        [ForeignKey("MusicGenreId")]
        public MusicGenre MusicGenre { get; set; }
    }
}
