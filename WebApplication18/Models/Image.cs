using System.Text.Json.Serialization;

namespace WebApplication18.Models;

public class Image
{
    public int Id { get; set; }
    public string Filename { get; set; }
    public string ContentType { get; set; }
    public long FileLength { get; set; }
    public string SupabasePublicUrl { get; set; }
    public string SupabasePath { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
        = DateTimeOffset.UtcNow;

    [JsonIgnore]
    public string Src =>
        $"/images/uploads/{SupabasePath}";

    
}