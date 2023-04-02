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


app.Run();

class MyDbContext : DbContext
{
    public virtual DbSet<ShortUrl> Urls { get; set; }

    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {}
}
