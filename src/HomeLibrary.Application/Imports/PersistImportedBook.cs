using HomeLibrary.Application.Interfaces;
using HomeLibrary.Domain;
using Microsoft.Extensions.Logging;

namespace HomeLibrary.Application.Imports;

public sealed class PersistImportedBook(
    IBookRepository bookRepository,
    ILogger<PersistImportedBook> logger)
{
    public async Task ExecuteAsync(BookImportMessage message, CancellationToken cancellationToken = default)
    {
        var book = Book.Create(message.Name, message.Author, message.Genre);
        await bookRepository.AddAsync(book, cancellationToken);
        logger.LogInformation(
            "Persisted imported book {Name} by {Author}",
            book.Name,
            book.Author);
    }
}
