using Baseline;
using Htmx;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Supabase.Storage;
using WebApplication18.Models;
using Client = Supabase.Client;
using FileOptions = Supabase.Storage.FileOptions;

namespace WebApplication18.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> logger;
    private readonly IDocumentStore db;
    private readonly Client supabase;

    public IReadOnlyList<Image> Images { get; set; }
        = new List<Image>().AsReadOnly();

    public IndexModel(ILogger<IndexModel> logger, IDocumentStore db, Client supabase)
    {
        this.logger = logger;
        this.db = db;
        this.supabase = supabase;
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
        await using var session = db.OpenSession();
        
        if (file is not null)
        {
            // upload to supabase
            var bucket = supabase.Storage.From(SupabaseConfiguration.Bucket);
            var bytes = await file.OpenReadStream().ReadAllBytesAsync();
            var extension = Path.GetExtension(file.FileName);

            var supabasePath = $"{Guid.NewGuid():N}{extension}";
            await bucket.Upload(bytes, supabasePath);

            var image = new Image {
                ContentType = MimeTypes.GetMimeType(file.FileName),
                Filename = file.FileName,
                FileLength = bytes.Length,
                SupabasePath = supabasePath
            };
         
            session.Store(image);
            await session.SaveChangesAsync();
            
            logger.LogInformation(
                "Stored the {Filename} with Id of {Id}", 
                image.Filename, image.Id
            );
        }

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