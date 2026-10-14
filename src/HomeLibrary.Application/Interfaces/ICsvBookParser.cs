using HomeLibrary.Application.Imports;

namespace HomeLibrary.Application.Interfaces;

public interface ICsvBookParser
{
    IReadOnlyList<ParsedBookRow> Parse(Stream csvStream);
}
