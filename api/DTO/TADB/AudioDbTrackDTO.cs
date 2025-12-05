namespace MediaMatch.DTO.TADB
{
    public class AudioDbTrackResponse
    {
        public List<AudioDbTrackDto>? track { get; set; }
    }

    public class AudioDbTrackDto
    {
        public string? idTrack { get; set; }
        public string? strTrack { get; set; }
        public string? intDuration { get; set; }
        public string? strTrackThumb { get; set; }
        public string? idAlbum { get; set; }
        public string? strGenre { get; set; }
        public string? strMood { get; set; }
        public string? strDescriptionEN { get; set; }
        public string? strMusicVid { get; set; }
    }
}
