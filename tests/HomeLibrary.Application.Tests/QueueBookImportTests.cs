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
    public async Task ExecuteAsync_with_parsed_rows_publishes_each()
    {
        _parser.Rows =
        [
            new ParsedBookRow("Dune", "Frank Herbert", "Science Fiction"),
            new ParsedBookRow("1984", "George Orwell", "Dystopian")
        ];

        await using var stream = new MemoryStream("csv"u8.ToArray());
        var result = await _sut.ExecuteAsync(
            stream, "text/csv", "books.csv", TestContext.Current.CancellationToken);

        Assert.Equal(2, result.QueuedCount);
        Assert.Equal(2, _publisher.Published.Count);
        Assert.Equal("Dune", _publisher.Published[0].Name);
        Assert.Equal("1984", _publisher.Published[1].Name);
    }

    [Fact]
    public async Task ExecuteAsync_with_text_plain_and_csv_filename_accepts()
    {
        _parser.Rows = [new ParsedBookRow("Dune", "Frank Herbert", "Science Fiction")];
        await using var stream = new MemoryStream("csv"u8.ToArray());

        var result = await _sut.ExecuteAsync(
            stream, "text/plain", "sample-books-10.csv", TestContext.Current.CancellationToken);

        Assert.Equal(1, result.QueuedCount);
    }

    [Fact]
    public async Task ExecuteAsync_with_csv_charset_content_type_accepts()
    {
        _parser.Rows = [new ParsedBookRow("Dune", "Frank Herbert", "Science Fiction")];
        await using var stream = new MemoryStream("csv"u8.ToArray());

        var result = await _sut.ExecuteAsync(
            stream, "text/csv; charset=utf-8", "books.csv", TestContext.Current.CancellationToken);

        Assert.Equal(1, result.QueuedCount);
    }

    [Fact]
    public async Task ExecuteAsync_without_file_throws()
    {
        var ex = await Assert.ThrowsAsync<UseCaseException>(() =>
            _sut.ExecuteAsync(null, "text/csv", cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("required", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(_publisher.Published);
    }

    [Fact]
    public async Task ExecuteAsync_with_wrong_type_and_non_csv_name_throws()
    {
        await using var stream = new MemoryStream("csv"u8.ToArray());

        var ex = await Assert.ThrowsAsync<UseCaseException>(() =>
            _sut.ExecuteAsync(stream, "application/json", "books.json", TestContext.Current.CancellationToken));
        Assert.Contains(".csv", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(_publisher.Published);
    }

    [Fact]
    public async Task ExecuteAsync_with_no_valid_rows_throws()
    {
        _parser.Rows = [];
        await using var stream = new MemoryStream("csv"u8.ToArray());

        var ex = await Assert.ThrowsAsync<UseCaseException>(() =>
            _sut.ExecuteAsync(stream, "text/csv", "books.csv", TestContext.Current.CancellationToken));
        Assert.Contains("no valid", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(_publisher.Published);
    }
}
