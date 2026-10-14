using HomeLibrary.Application.Books;
using HomeLibrary.Application.Tests.Fakes;
using HomeLibrary.Domain;

namespace HomeLibrary.Application.Tests;

public class ListBooksTests
{
    [Fact]
    public async Task ExecuteAsync_returns_books_newest_first()
    {
        var repository = new FakeBookRepository();
        var older = Book.Create("Older", "A", "G", new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var newer = Book.Create("Newer", "B", "G", new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero));
        repository.Seed(
            Book.Restore(1, older.Name, older.Author, older.Genre, older.ImportDate),
            Book.Restore(2, newer.Name, newer.Author, newer.Genre, newer.ImportDate));

        var sut = new ListBooks(repository);
        var result = await sut.ExecuteAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Newer", result[0].Name);
        Assert.Equal("Older", result[1].Name);
        Assert.Equal(2, result[0].Id);
    }

    [Fact]
    public async Task ExecuteAsync_when_empty_returns_empty_list()
    {
        var sut = new ListBooks(new FakeBookRepository());

        var result = await sut.ExecuteAsync();

        Assert.Empty(result);
    }
}
