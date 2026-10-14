using HomeLibrary.Application.Abstractions;
using HomeLibrary.Application.Imports;

namespace HomeLibrary.Application.Tests.Fakes;

public sealed class FakeBookImportPublisher : IBookImportPublisher
{
    public List<BookImportMessage> Published { get; } = [];

    public Task PublishAsync(BookImportMessage message, CancellationToken cancellationToken = default)
    {
        Published.Add(message);
        return Task.CompletedTask;
    }
}
