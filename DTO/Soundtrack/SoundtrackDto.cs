namespace MediaMatch.DTO.Soundtrack
{
    public class SoundtrackDto
    {
        public string source { get; set; }
        public string composer { get; set; }
        public SoundtrackAlbumDto album { get; set; }
        public List<SoundtrackTrackDto> tracks { get; set; }
        public string confidence { get; set; }
    }

    public class SoundtrackAlbumDto
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string release_date { get; set; }
        public List<string> artists { get; set; }
    }

    public class SoundtrackTrackDto
    {
        public string id { get; set; }
        public string title { get; set; }
        public List<string> artists { get; set; }
        public int duration_ms { get; set; }
        public string url { get; set; }
        public string preview_url { get; set; }
        public string genre { get; set; }
        public string mood { get; set; }
        public string description { get; set; }
        public string thumb_url { get; set; }
        public string video_url { get; set; }
    }
}