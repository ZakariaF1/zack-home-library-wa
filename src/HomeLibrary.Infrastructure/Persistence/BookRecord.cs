namespace HomeLibrary.Infrastructure.Persistence;

public sealed class BookRecord
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public DateTimeOffset ImportDate { get; set; }
}
