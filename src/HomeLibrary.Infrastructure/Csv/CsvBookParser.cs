using System.Globalization;
using System.Text;
using HomeLibrary.Application;
using HomeLibrary.Application.Interfaces;
using HomeLibrary.Application.Imports;

namespace HomeLibrary.Infrastructure.Csv;

public sealed class CsvBookParser : ICsvBookParser
{
    public IReadOnlyList<ParsedBookRow> Parse(Stream csvStream)
    {
        using var reader = new StreamReader(csvStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        var header = reader.ReadLine();
        if (header is null)
        {
            return [];
        }

        if (!LooksLikeHeader(header))
        {
            throw new UseCaseException("CSV must include a header row with name, author, and genre.");
        }

        var rows = new List<ParsedBookRow>();
        while (reader.ReadLine() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var fields = SplitCsvLine(line);
            if (fields.Count < 3)
            {
                continue;
            }

            var name = fields[0].Trim();
            var author = fields[1].Trim();
            var genre = fields[2].Trim();
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(author) || string.IsNullOrWhiteSpace(genre))
            {
                continue;
            }

            rows.Add(new ParsedBookRow(name, author, genre));
        }

        return rows;
    }

    private static bool LooksLikeHeader(string headerLine)
    {
        var fields = SplitCsvLine(headerLine);
        if (fields.Count < 3)
        {
            return false;
        }

        return fields[0].Trim().Equals("name", StringComparison.OrdinalIgnoreCase)
               && fields[1].Trim().Equals("author", StringComparison.OrdinalIgnoreCase)
               && fields[2].Trim().Equals("genre", StringComparison.OrdinalIgnoreCase);
    }

    private static List<string> SplitCsvLine(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (ch == ',' && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(ch);
        }

        fields.Add(current.ToString());
        return fields;
    }
}
