namespace HomeLibrary.Domain;

public sealed class Book
{
    private Book(long id, string name, string author, string genre, DateTimeOffset importDate)
    {
        Id = id;
        Name = name;
        Author = author;
        Genre = genre;
        ImportDate = importDate;
    }

    public long Id { get; }
    public string Name { get; }
    public string Author { get; }
    public string Genre { get; }
    public DateTimeOffset ImportDate { get; }

    public static Book Create(string name, string author, string genre, DateTimeOffset? importDate = null)
    {
        var normalizedName = NormalizeRequired(name, nameof(name));
        var normalizedAuthor = NormalizeRequired(author, nameof(author));
        var normalizedGenre = NormalizeRequired(genre, nameof(genre));
        var when = importDate ?? DateTimeOffset.UtcNow;

        return new Book(0, normalizedName, normalizedAuthor, normalizedGenre, when.ToUniversalTime());
    }

    public static Book Restore(long id, string name, string author, string genre, DateTimeOffset importDate)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        return new Book(
            id,
            NormalizeRequired(name, nameof(name)),
            NormalizeRequired(author, nameof(author)),
            NormalizeRequired(genre, nameof(genre)),
            importDate.ToUniversalTime());
    }

    private static string NormalizeRequired(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{fieldName} is required.");
        }

        return value.Trim();
    }
}
