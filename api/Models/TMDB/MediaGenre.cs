using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaMatch.Models.TMDB
{
    public class MediaGenre
    {
        public int MediaItemId { get; set; }
        [ForeignKey("MediaItemId")]
        public MediaItem MediaItem { get; set; }

        public int GenreId { get; set; }
        [ForeignKey("GenreId")]
        public Genre Genre { get; set; }
    }
}