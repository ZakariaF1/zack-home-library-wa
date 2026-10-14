using HomeLibrary.Application;
using HomeLibrary.Application.Books;
using HomeLibrary.Application.Imports;
using HomeLibrary.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHomeLibraryInfrastructure(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

await app.Services.InitializeHomeLibraryAsync();

app.UseCors();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapPost("/api/imports", async (
    HttpRequest request,
    QueueBookImport queueBookImport,
    CancellationToken cancellationToken) =>
{
    if (!request.HasFormContentType)
    {
        return Results.StatusCode(StatusCodes.Status415UnsupportedMediaType);
    }

    var form = await request.ReadFormAsync(cancellationToken);
    var file = form.Files.GetFile("file") ?? form.Files.FirstOrDefault();
    if (file is null || file.Length == 0)
    {
        return Results.BadRequest(new { error = "A CSV file is required." });
    }

    try
    {
        var contentType = file.ContentType;
        if (string.IsNullOrWhiteSpace(contentType)
            || contentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
        {
            contentType = file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
                ? "text/csv"
                : contentType;
        }

        await using var stream = file.OpenReadStream();
        var result = await queueBookImport.ExecuteAsync(stream, contentType, cancellationToken);
        // Explicit JSON + 202 so clients reliably receive the body.
        return Results.Json(new { queuedCount = result.QueuedCount }, statusCode: StatusCodes.Status202Accepted);
    }
    catch (UseCaseException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.DisableAntiforgery();

app.MapGet("/api/books", async (ListBooks listBooks, CancellationToken cancellationToken) =>
{
    var books = await listBooks.ExecuteAsync(cancellationToken);
    return Results.Ok(books);
});

app.Run();

public partial class Program;
