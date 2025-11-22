namespace MediaMatch.DTO.TADB
{
    public class AudioDbAlbumResponse
    {
        public List<AudioDbAlbumDto>? album { get; set; }
    }

    public class AudioDbAlbumDto
    {
        public string? idAlbum { get; set; }
        public string? strAlbum { get; set; }
        public string? intYearReleased { get; set; }
        public string? strGenre { get; set; }
        public string? strLabel { get; set; }
        public string? strAlbumThumb { get; set; }
        public string? idArtist { get; set; } // Relacionamento
    }
}
