using HomeLibrary.Application.Abstractions;
using HomeLibrary.Application.Imports;

namespace HomeLibrary.Application.Tests.Fakes;

public sealed class FakeCsvBookParser : ICsvBookParser
{
    public IReadOnlyList<ParsedBookRow> Rows { get; set; } = [];

    public Exception? ExceptionToThrow { get; set; }

    public IReadOnlyList<ParsedBookRow> Parse(Stream csvStream)
    {
        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }

        _ = csvStream;
        return Rows;
    }
}
