using System.Text;
using HomeLibrary.Application;
using HomeLibrary.Infrastructure.Csv;

namespace HomeLibrary.Application.Tests;

public class CsvBookParserTests
{
    private readonly CsvBookParser _sut = new();

    [Fact]
    public void Parse_with_incomplete_rows_skips_them_and_returns_valid()
    {
        const string csv = """
            name,author,genre
            Dune,Frank Herbert,Science Fiction
            Incomplete,OnlyAuthor,
            1984,George Orwell,Dystopian
            """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        var rows = _sut.Parse(stream);

        Assert.Equal(2, rows.Count);
        Assert.Equal("Dune", rows[0].Name);
        Assert.Equal("1984", rows[1].Name);
    }

    [Fact]
    public void Parse_with_missing_header_throws()
    {
        const string csv = "Dune,Frank Herbert,Science Fiction";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        Assert.Throws<UseCaseException>(() => _sut.Parse(stream));
    }
}
