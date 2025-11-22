namespace MediaMatch.DTO.TADB
{
    public class AudioDbGenreResponse
    {
        public List<AudioDbGenreDto>? genres { get; set; }
    }

    public class AudioDbGenreDto
    {
        public string? idGenre { get; set; }
        public string? strGenre { get; set; }

    }
}
