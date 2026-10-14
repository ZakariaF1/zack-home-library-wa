using HomeLibrary.Application;
using HomeLibrary.Application.Imports;
using HomeLibrary.Application.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace HomeLibrary.Application.Tests;

public class QueueBookImportTests
{
    private readonly FakeCsvBookParser _parser = new();
    private readonly FakeBookImportPublisher _publisher = new();
    private readonly QueueBookImport _sut;

    public QueueBookImportTests()
    {
        _sut = new QueueBookImport(_parser, _publisher, NullLogger<QueueBookImport>.Instance);
    }

    [Fact]
    public async Task ExecuteAsync_publishes_each_parsed_row()
    {
        _parser.Rows =
        [
            new ParsedBookRow("Dune", "Frank Herbert", "Science Fiction"),
            new ParsedBookRow("1984", "George Orwell", "Dystopian")
        ];

        await using var stream = new MemoryStream("csv"u8.ToArray());
        var result = await _sut.ExecuteAsync(stream, "text/csv", "books.csv");

        Assert.Equal(2, result.QueuedCount);
        Assert.Equal(2, _publisher.Published.Count);
        Assert.Equal("Dune", _publisher.Published[0].Name);
        Assert.Equal("1984", _publisher.Published[1].Name);
    }

    [Fact]
    public async Task ExecuteAsync_accepts_browser_text_plain_when_filename_is_csv()
    {
        _parser.Rows = [new ParsedBookRow("Dune", "Frank Herbert", "Science Fiction")];
        await using var stream = new MemoryStream("csv"u8.ToArray());

        var result = await _sut.ExecuteAsync(stream, "text/plain", "sample-books-10.csv");

        Assert.Equal(1, result.QueuedCount);
    }

    [Fact]
    public async Task ExecuteAsync_accepts_csv_content_type_with_charset()
    {
        _parser.Rows = [new ParsedBookRow("Dune", "Frank Herbert", "Science Fiction")];
        await using var stream = new MemoryStream("csv"u8.ToArray());

        var result = await _sut.ExecuteAsync(stream, "text/csv; charset=utf-8", "books.csv");

        Assert.Equal(1, result.QueuedCount);
    }

    [Fact]
    public async Task ExecuteAsync_without_file_throws()
    {
        var ex = await Assert.ThrowsAsync<UseCaseException>(() => _sut.ExecuteAsync(null, "text/csv"));
        Assert.Contains("required", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(_publisher.Published);
    }

    [Fact]
    public async Task ExecuteAsync_with_wrong_type_and_non_csv_name_throws()
    {
        await using var stream = new MemoryStream("csv"u8.ToArray());

        var ex = await Assert.ThrowsAsync<UseCaseException>(
            () => _sut.ExecuteAsync(stream, "application/json", "books.json"));
        Assert.Contains(".csv", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(_publisher.Published);
    }

    [Fact]
    public async Task ExecuteAsync_with_no_valid_rows_throws()
    {
        _parser.Rows = [];
        await using var stream = new MemoryStream("csv"u8.ToArray());

        var ex = await Assert.ThrowsAsync<UseCaseException>(() => _sut.ExecuteAsync(stream, "text/csv", "books.csv"));
        Assert.Contains("no valid", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(_publisher.Published);
    }
}
