using HomeLibrary.Application.Abstractions;
using HomeLibrary.Domain;
using Microsoft.EntityFrameworkCore;

namespace HomeLibrary.Infrastructure.Persistence;

public sealed class EfBookRepository(LibraryDbContext dbContext) : IBookRepository
{
    public async Task AddAsync(Book book, CancellationToken cancellationToken = default)
    {
        dbContext.Books.Add(new BookRecord
        {
            Name = book.Name,
            Author = book.Author,
            Genre = book.Genre,
            ImportDate = book.ImportDate
        });
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Book>> ListByImportDateDescAsync(CancellationToken cancellationToken = default)
    {
        var rows = await dbContext.Books
            .AsNoTracking()
            .OrderByDescending(book => book.ImportDate)
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => Book.Restore(row.Id, row.Name, row.Author, row.Genre, row.ImportDate))
            .ToList();
    }
}
