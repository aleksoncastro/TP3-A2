using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MediaMatch.DTO.TADB
{
    // Classe genérica necessária para ler o JSON { "loved": [...] }
    public class AudioDbLovedResponse<T>
    {
        public List<T>? loved { get; set; }
    }
}