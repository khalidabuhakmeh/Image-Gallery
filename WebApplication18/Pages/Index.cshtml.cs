using Baseline;
using Htmx;
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

    public async Task<IActionResult> OnPost([FromForm] IFormFile? file)
    {
        if (file is null) {
            return RedirectToPage("Index");
        }
        
        var image = new Image
        {
            ContentType = MimeTypes.GetMimeType(file.FileName),
            Filename = file.FileName,
            Bytes = await file.OpenReadStream().ReadAllBytesAsync()
        };

        await using var session = db.OpenSession();
        session.Store(image);
        await session.SaveChangesAsync();
        
        logger.LogInformation(
            "Stored the {Filename} with Id of {Id}", 
            image.Filename, image.Id
        );

        Images = await session
            .Query<Image>()
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync();

        // graceful fallback
        return Request.IsHtmx()
            ? Partial("_Images", this)
            : RedirectToPage("Index");
    }
}