using Microsoft.AspNetCore.WebUtilities;

namespace mercedes_coding_challenge.Models
{
    public class ShortUrl
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string UrlChunk { get; set; } = string.Empty;
    }
}
