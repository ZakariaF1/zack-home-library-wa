using HomeLibrary.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace HomeLibrary.Application.Imports;

public sealed class QueueBookImport(
    ICsvBookParser csvBookParser,
    IBookImportPublisher bookImportPublisher,
    ILogger<QueueBookImport> logger)
{
    public async Task<QueueBookImportResult> ExecuteAsync(
        Stream? csvStream,
        string? contentType,
        string? fileName = null,
        CancellationToken cancellationToken = default)
    {
        if (csvStream is null)
        {
            throw new UseCaseException("A CSV file is required.");
        }

        if (!IsAcceptableCsvUpload(contentType, fileName))
        {
            throw new UseCaseException("Upload a .csv file (name, author, genre).");
        }

        var rows = csvBookParser.Parse(csvStream);
        if (rows.Count == 0)
        {
            throw new UseCaseException("CSV contained no valid book rows.");
        }

        var queuedCount = 0;
        foreach (var row in rows)
        {
            await bookImportPublisher.PublishAsync(
                new BookImportMessage(row.Name, row.Author, row.Genre),
                cancellationToken);
            queuedCount++;
        }

        logger.LogInformation("Queued {QueuedCount} book import messages", queuedCount);
        return new QueueBookImportResult(queuedCount);
    }

    private static bool IsAcceptableCsvUpload(string? contentType, string? fileName)
    {
        var hasCsvExtension = !string.IsNullOrWhiteSpace(fileName)
            && fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);

        // Browsers often send CSV as text/plain or application/octet-stream.
        if (hasCsvExtension)
        {
            return true;
        }

        return IsCsvContentType(contentType);
    }

    private static bool IsCsvContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return false;
        }

        var mediaType = contentType.Split(';', 2)[0].Trim();
        return mediaType.Equals("text/csv", StringComparison.OrdinalIgnoreCase)
               || mediaType.Equals("application/csv", StringComparison.OrdinalIgnoreCase)
               || mediaType.Equals("application/vnd.ms-excel", StringComparison.OrdinalIgnoreCase)
               || mediaType.Equals("text/plain", StringComparison.OrdinalIgnoreCase)
               || mediaType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase);
    }
}

public sealed record QueueBookImportResult(int QueuedCount);
