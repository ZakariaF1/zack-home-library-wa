using HomeLibrary.Application.Imports;
using HomeLibrary.Application.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace HomeLibrary.Application.Tests;

public class PersistImportedBookTests
{
    [Fact]
    public async Task ExecuteAsync_with_valid_message_adds_book_to_repository()
    {
        var repository = new FakeBookRepository();
        var sut = new PersistImportedBook(repository, NullLogger<PersistImportedBook>.Instance);

        await sut.ExecuteAsync(
            new BookImportMessage("Dune", "Frank Herbert", "Science Fiction"),
            TestContext.Current.CancellationToken);

        Assert.Single(repository.Books);
        Assert.Equal("Dune", repository.Books[0].Name);
        Assert.Equal("Frank Herbert", repository.Books[0].Author);
    }
}
