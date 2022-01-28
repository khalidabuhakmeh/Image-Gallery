using Marten;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Providers;
using Weasel.Postgresql;
using WebApplication18;

var builder = WebApplication.CreateBuilder(args);
var supabase = new SupabaseConfiguration();
builder.Configuration.Bind("Supabase", supabase);
await Supabase.Client.InitializeAsync(supabase.Url, supabase.Key);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddWebOptimizer();
builder.Services.AddSingleton(_ => Supabase.Client.Instance);

builder.Services.AddMarten(options => {
    options.Connection(supabase.ConnectionString);
    // destructive, but YOLO!
    options.AutoCreateSchemaObjects = AutoCreate.All;
});

builder.Services
    .AddImageSharp()
    .ClearProviders()
    .AddProvider<SupabaseImageProvider>()
    .AddProvider<PhysicalFileSystemProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseImageSharp();
app.UseWebOptimizer();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();

public class SupabaseConfiguration {
    public const string Bucket = "helloworld-images";
    
    public string Url { get; init; }
    public string Key { get; init; }
    public string ConnectionString { get; init; }
}
