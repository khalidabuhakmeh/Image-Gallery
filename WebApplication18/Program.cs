using Marten;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Providers;
using Weasel.Postgresql;
using WebApplication18;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddWebOptimizer();
builder.Services.AddMarten(options => {
    var connectionString = builder.Configuration.GetConnectionString("marten");
    options.Connection(connectionString);
    // destructive, but YOLO!
    options.AutoCreateSchemaObjects = AutoCreate.All;
});

builder.Services
    .AddImageSharp()
    .ClearProviders()
    .AddProvider<MartenImageProvider>()
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

