using System.Text.RegularExpressions;
using Marten;
using Microsoft.AspNetCore.Http.Extensions;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Providers;
using SixLabors.ImageSharp.Web.Resolvers;
using Supabase.Storage;
using WebApplication18.Models;
using Client = Supabase.Client;

namespace WebApplication18;

// ReSharper disable once ClassNeverInstantiated.Global
public class SupabaseImageProvider : IImageProvider
{
    private readonly FormatUtilities formatUtilities;
    private readonly IDocumentStore store;

    private static readonly Regex UrlPattern 
        = new("^/images/uploads/(?<id>.+)$", RegexOptions.IgnoreCase);

    public SupabaseImageProvider(FormatUtilities formatUtilities, IDocumentStore store)
    {
        this.formatUtilities = formatUtilities;
        this.store = store;
    }
    
    public bool IsValidRequest(HttpContext context) 
        => formatUtilities.GetExtensionFromUri(context.Request.GetDisplayUrl()) != null;

    public async Task<IImageResolver> GetAsync(HttpContext context)
    {
        await using var session = store.LightweightSession();

        var match = UrlPattern.Match(context.Request.Path);
        var id = match.Groups["id"].Value;

        var image = await session.Query<Image>()
            .Where(i => i.SupabasePath == id)
            .FirstOrDefaultAsync();
        
        if (image is null) {
            return null!;
        }
        
        var resolver = new SupabaseResolver(image);
        return resolver;
    }

    public ProcessingBehavior ProcessingBehavior 
        => ProcessingBehavior.All;

    public Func<HttpContext, bool> Match { get; set; } 
        = ctx =>
        {
            var isMatch = UrlPattern.IsMatch(ctx.Request.Path);
            return isMatch;
        };

    private class SupabaseResolver : IImageResolver
    {
        private readonly Image image;
        private readonly Client client;

        public SupabaseResolver(Image image)
        {
            this.image = image;
            client = Client.Instance;
        }
        
        public Task<ImageMetadata> GetMetaDataAsync()
        {
            return Task.FromResult(new ImageMetadata(image.UpdatedAt.DateTime, image.FileLength));
        }

        public async Task<Stream> OpenReadAsync()
        {
            var bucket = client.Storage.From(SupabaseConfiguration.Bucket);
            var bytes = await bucket.Download(image.SupabasePath);
            return new MemoryStream(bytes);
        }
    }
}