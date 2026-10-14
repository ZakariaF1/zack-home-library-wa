using HomeLibrary.Application.Imports;

namespace HomeLibrary.Application.Abstractions;

public interface ICsvBookParser
{
    IReadOnlyList<ParsedBookRow> Parse(Stream csvStream);
}
