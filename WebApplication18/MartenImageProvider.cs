using System.Text.RegularExpressions;
using Marten;
using Microsoft.AspNetCore.Http.Extensions;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Providers;
using SixLabors.ImageSharp.Web.Resolvers;
using WebApplication18.Models;

namespace WebApplication18;

// ReSharper disable once ClassNeverInstantiated.Global
public class MartenImageProvider : IImageProvider
{
    private readonly FormatUtilities formatUtilities;
    private readonly IDocumentStore store;

    private static readonly Regex UrlPattern 
        = new("^/images/uploads/(?<id>[0-9]+)/(?<filename>.+\\..+)$", RegexOptions.IgnoreCase);

    public MartenImageProvider(FormatUtilities formatUtilities, IDocumentStore store)
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
        var id = int.Parse(match.Groups["id"].Value);

        var image = await session.LoadAsync<Image>(id);
        if (image is null) {
            return null!;
        }
        
        var resolver = new InMemoryResolver(image.Bytes, image.UpdatedAt.Date);
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

    private class InMemoryResolver : IImageResolver
    {
        private readonly byte[] bytes;
        private readonly DateTime modified;

        public InMemoryResolver(byte[] bytes, DateTime modified)
        {
            this.bytes = bytes;
            this.modified = modified;
        }
        
        public Task<ImageMetadata> GetMetaDataAsync()
        {
            return Task.FromResult(new ImageMetadata(modified, bytes.Length));
        }

        public Task<Stream> OpenReadAsync()
        {
            return Task.FromResult((Stream) new MemoryStream(bytes));
        }
    }
}