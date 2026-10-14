using HomeLibrary.Application.Interfaces;

namespace HomeLibrary.Application.Books;

public sealed class ListBooks(IBookRepository bookRepository)
{
    public async Task<IReadOnlyList<BookDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var books = await bookRepository.ListByImportDateDescAsync(cancellationToken);
        return books
            .Select(book => new BookDto(book.Id, book.Name, book.Author, book.Genre, book.ImportDate))
            .ToList();
    }
}
