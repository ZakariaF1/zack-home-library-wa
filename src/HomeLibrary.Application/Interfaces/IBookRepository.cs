using HomeLibrary.Domain;

namespace HomeLibrary.Application.Interfaces;

public interface IBookRepository
{
    Task<IReadOnlyList<Book>> ListByImportDateDescAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Book book, CancellationToken cancellationToken = default);
}
