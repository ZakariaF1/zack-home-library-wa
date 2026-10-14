using Microsoft.EntityFrameworkCore;

namespace HomeLibrary.Infrastructure.Persistence;

public sealed class LibraryDbContext(DbContextOptions<LibraryDbContext> options) : DbContext(options)
{
    public DbSet<BookRecord> Books => Set<BookRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var book = modelBuilder.Entity<BookRecord>();
        book.ToTable("library");
        book.HasKey(x => x.Id);
        book.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        book.Property(x => x.Name).HasColumnName("name").IsRequired();
        book.Property(x => x.Author).HasColumnName("author").IsRequired();
        book.Property(x => x.Genre).HasColumnName("genre").IsRequired();
        book.Property(x => x.ImportDate).HasColumnName("import_date").IsRequired();
    }
}
