using System.Net.Mime;
using Baseline;
using Htmx;
using JetBrains.Annotations;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication18.Models;

namespace WebApplication18.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> logger;
    private readonly IDocumentStore db;

    public IReadOnlyList<Image> Images { get; set; }
        = new List<Image>().AsReadOnly();

    public IndexModel(ILogger<IndexModel> logger, IDocumentStore db)
    {
        this.logger = logger;
        this.db = db;
    }

    public async Task OnGet()
    {
        await using var session = db.LightweightSession();
        Images = await session
            .Query<Image>()
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPost([FromForm] IFormFile file)
    {
        var image = new Image {
            ContentType = MimeTypes.GetMimeType(file.FileName),
            Filename = file.FileName,
            Bytes = await file.OpenReadStream().ReadAllBytesAsync()
        };

        await using var session = db.OpenSession();
        session.Store(image);
        await session.SaveChangesAsync();

        Images = await session
            .Query<Image>()
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync();

        return Partial("_Images", this);
    }
}

public static class PageModelExtensions
{
    public static IActionResult Html(this PageModel model,
        [LanguageInjection(InjectedLanguage.HTML)]
        string html)
    {
        return model.Content(html, "text/html");
    }

    public static IActionResult Html2(this PageModel model, string html)
    {
        return model.Content(html, "text/html");
    }
}