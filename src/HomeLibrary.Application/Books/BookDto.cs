namespace HomeLibrary.Application.Books;

public sealed record BookDto(long Id, string Name, string Author, string Genre, DateTimeOffset ImportDate);
