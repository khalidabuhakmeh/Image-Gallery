using System.Text.Json.Serialization;

namespace WebApplication18.Models;

public class Image
{
    public int Id { get; set; }
    public byte[] Bytes { get; set; }
    public string Filename { get; set; }
    public string ContentType { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
        = DateTimeOffset.UtcNow;

    [JsonIgnore]
    public string Src =>
        $"/images/uploads/{Id}/{Filename}";
}