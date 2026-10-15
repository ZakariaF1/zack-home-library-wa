using HomeLibrary.Domain;

namespace HomeLibrary.Domain.Tests;

public class BookTests
{
    [Fact]
    public void Create_with_valid_fields_sets_properties_and_utc_import_date()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);

        var book = Book.Create("Dune", "Frank Herbert", "Science Fiction");

        var after = DateTimeOffset.UtcNow.AddSeconds(1);
        Assert.Equal("Dune", book.Name);
        Assert.Equal("Frank Herbert", book.Author);
        Assert.Equal("Science Fiction", book.Genre);
        Assert.True(book.ImportDate >= before && book.ImportDate <= after);
        Assert.Equal(TimeSpan.Zero, book.ImportDate.Offset);
    }

    [Fact]
    public void Create_with_explicit_import_date_preserves_it()
    {
        var when = new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero);

        var book = Book.Create("Dune", "Frank Herbert", "Science Fiction", when);

        Assert.Equal(when, book.ImportDate);
    }

    [Theory]
    [InlineData(null, "Author", "Genre")]
    [InlineData("", "Author", "Genre")]
    [InlineData("   ", "Author", "Genre")]
    [InlineData("Name", null, "Genre")]
    [InlineData("Name", "", "Genre")]
    [InlineData("Name", "   ", "Genre")]
    [InlineData("Name", "Author", null)]
    [InlineData("Name", "Author", "")]
    [InlineData("Name", "Author", "   ")]
    public void Create_with_missing_fields_throws(string? name, string? author, string? genre)
    {
        Assert.Throws<DomainException>(() => Book.Create(name!, author!, genre!));
    }

    [Fact]
    public void Create_with_padded_whitespace_trims_fields()
    {
        var book = Book.Create("  Dune  ", "  Frank Herbert  ", "  Science Fiction  ");

        Assert.Equal("Dune", book.Name);
        Assert.Equal("Frank Herbert", book.Author);
        Assert.Equal("Science Fiction", book.Genre);
    }
}
