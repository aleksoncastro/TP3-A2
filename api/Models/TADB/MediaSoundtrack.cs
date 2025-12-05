using MediaMatch.Models.TMDB;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaMatch.Models.TADB
{
    public class MediaSoundtrack
    {
        public int MediaItemId { get; set; }
        [ForeignKey("MediaItemId")]
        public MediaItem MediaItem { get; set; }

        public int TrackId { get; set; }
        [ForeignKey("TrackId")]
        public Track Track { get; set; }

        // Quem adicionou essa música ao filme/série
        public int AddedBy { get; set; }
        [ForeignKey("AddedBy")]
        public User User { get; set; }
    }
}