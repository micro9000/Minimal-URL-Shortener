using LiteDB;
using Microsoft.AspNetCore.WebUtilities;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ILiteDatabase, LiteDatabase>(_ => new LiteDatabase("short-links.db"));
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Home page: A form for submitting a URL
app.MapGet("/", () => Results.File("index.html", "text/html"));

// API endpoint for shortening a URL and save it to a local database
app.MapPost("/url", (UrlDto request, ILiteDatabase liteDb, HttpRequest httpRequest) =>
{
    if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var inputUri))
    {
        return Results.BadRequest(new { Errors = "URL is invalid." });
    }

    var links = liteDb.GetCollection<ShortUrl>(BsonAutoId.Int32);
    var entry = new ShortUrl(inputUri);
    links.Insert(entry);

    var url = $"{httpRequest.Scheme}://{httpRequest.Host}/{entry.UrlChunk}";

    return Results.Ok(new { url });
});

// Catch all page: redirecting shortened URL to its original address
app.MapFallback((HttpRequest httpRequest, ILiteDatabase db) =>
{
    var collection = db.GetCollection<ShortUrl>();

    var path = httpRequest.Path.ToUriComponent().Trim('/');
    var id = BitConverter.ToInt32(WebEncoders.Base64UrlDecode(path));
    var entry = collection.Find(p => p.Id == id).FirstOrDefault();

    return Results.Redirect(entry?.Url ?? "/");
}); 

app.Run();

public class ShortUrl
{
    public int Id { get; protected set; }
    public string Url { get; protected set; }
    public string UrlChunk => WebEncoders.Base64UrlEncode(BitConverter.GetBytes(Id));

    public ShortUrl(Uri url)
    {
        Url = url.ToString();
    }
}

public class UrlDto
{
    public string Url { get; set; }
}