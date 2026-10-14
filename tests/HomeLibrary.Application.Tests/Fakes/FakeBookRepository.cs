using HomeLibrary.Application.Interfaces;
using HomeLibrary.Domain;

namespace HomeLibrary.Application.Tests.Fakes;

public sealed class FakeBookRepository : IBookRepository
{
    private readonly List<Book> _books = [];

    public IReadOnlyList<Book> Books => _books;

    public Task AddAsync(Book book, CancellationToken cancellationToken = default)
    {
        _books.Add(book);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Book>> ListByImportDateDescAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Book> result = _books
            .OrderByDescending(book => book.ImportDate)
            .ToList();
        return Task.FromResult(result);
    }

    public void Seed(params Book[] books) => _books.AddRange(books);
}
