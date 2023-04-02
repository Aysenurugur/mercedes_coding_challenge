using mercedes_coding_challenge.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string connStr = builder.Configuration.GetConnectionString(name: "DefaultConnection");
builder.Services.AddDbContext<MyDbContext>(optionsAction: options => options.UseSqlite(connStr));

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// API endpoint for shortening a URL and save it to a local database
app.MapPost("/url", ShortenerDelegate);

// Catch all page: redirecting shortened URL to its original address
app.MapFallback(RedirectDelegate);

static async Task ShortenerDelegate(HttpContext httpContext)
{
    var request = await httpContext.Request.ReadFromJsonAsync<UrlDto>() ?? new UrlDto();

    // Validating input URL
    if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var inputUri))
    {
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsync("URL is invalid.");
        return;
    }

    var sqLiteDb = httpContext.RequestServices.GetRequiredService<MyDbContext>();
    var entry = new ShortUrl()
    {
        Url = inputUri.ToString()
    };
    sqLiteDb.Urls.Add(entry);

    //Saving entry to create entry's Id
    await sqLiteDb.SaveChangesAsync();
    //Hashing URL from entry's Id
    entry.UrlChunk = WebEncoders.Base64UrlEncode(BitConverter.GetBytes(entry.Id));
    await sqLiteDb.SaveChangesAsync();


    var result = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/{entry.UrlChunk}";
    await httpContext.Response.WriteAsJsonAsync(new { url = result });
}

static async Task RedirectDelegate(HttpContext httpContext)
{
    var db = httpContext.RequestServices.GetRequiredService<MyDbContext>();
    var collection = db.Urls;

    var path = httpContext.Request.Path.ToUriComponent().Trim('/');
    var id = BitConverter.ToInt32(WebEncoders.Base64UrlDecode(path));
    var entry = collection.FirstOrDefault(p => p.Id == id);

    httpContext.Response.Redirect(entry?.Url ?? "/");

    await Task.CompletedTask;
}

app.Run();

class MyDbContext : DbContext
{
    public virtual DbSet<ShortUrl> Urls { get; set; }

    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {}
}
