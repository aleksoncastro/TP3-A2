namespace MediaMatch.DTO.TADB
{
    public class AudioDbArtistResponse
    {
        public List<AudioDbArtistDto>? artists { get; set; }
    }

    public class AudioDbArtistDto
    {
        public string? idArtist { get; set; }
        public string? strArtist { get; set; }
        public string? strBiographyEN { get; set; }
        public string? strCountry { get; set; }
        public string? strArtistThumb { get; set; }
    }
}
