using mercedes_coding_challenge.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string connStr = builder.Configuration.GetConnectionString(name: "DefaultConnection") ?? string.Empty;
builder.Services.AddDbContext<MyDbContext>(optionsAction: options => options.UseSqlite(connStr));

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//API endpoint for shortening a URL and save it to a local database
app.MapPost(pattern: "/url_shortener", handler: async (UrlShortenerDto url, HttpContext httpContext) =>
{
    //Validating input URL
    if (!Uri.TryCreate(url.Url, UriKind.Absolute, out var inputUri))
    {
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsync("URL is invalid.");
        return;
    }

    //Creating random url chunk
    var rnd = new Random();
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ@1234567890az";
    var randomStr = new string(Enumerable.Repeat(chars, 6).Select(s => s[rnd.Next(s.Length)]).ToArray());

    var sqLiteDb = httpContext.RequestServices.GetRequiredService<MyDbContext>();
    var entry = new ShortUrl()
    {
        Url = inputUri.ToString(),
        UrlChunk = randomStr
    };
    sqLiteDb.Urls.Add(entry);
    await sqLiteDb.SaveChangesAsync();


    var result = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/{entry.UrlChunk}";
    await httpContext.Response.WriteAsJsonAsync(new UrlShortResponseDto() { Url = result });
});

//API endpoint for customizing a URL and save it to a local database
app.MapPost(pattern: "/url_customizer", handler: async (UrlCustomizerDto url, HttpContext httpContext) =>
{
    //Validating input URL
    if (!Uri.TryCreate(url.BaseUrl, UriKind.Absolute, out var inputUri))
    {
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsync("URL is invalid.");
        return;
    }

    var sqLiteDb = httpContext.RequestServices.GetRequiredService<MyDbContext>();

    //Unique custom URL check
    if(sqLiteDb.Urls.Any(s => s.UrlChunk == url.CustomUrlChunk.Trim()))
    {
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsync("Custom URL is taken.");
        return;
    }

    var entry = new ShortUrl()
    {
        Url = inputUri.ToString(),
        UrlChunk = url.CustomUrlChunk.Trim()
    };
    sqLiteDb.Urls.Add(entry);
    await sqLiteDb.SaveChangesAsync();


    var result = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/{entry.UrlChunk}";
    await httpContext.Response.WriteAsJsonAsync(new UrlCustomResponseDto() { Url = result });
});

//Catch all page: redirecting shortened URL to its original address
app.MapFallback(handler: async (HttpContext httpContext) =>
{
    var db = httpContext.RequestServices.GetRequiredService<MyDbContext>();
    var collection = db.Urls;

    var urlChunk = httpContext.Request.Path.ToUriComponent().Trim('/');
    var entry = collection.FirstOrDefault(p => p.UrlChunk == urlChunk);

    httpContext.Response.Redirect(entry?.Url ?? "/");

    await Task.CompletedTask;
});

app.Run();

class MyDbContext : DbContext
{
    public virtual DbSet<ShortUrl> Urls { get; set; }

    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    { }
}
