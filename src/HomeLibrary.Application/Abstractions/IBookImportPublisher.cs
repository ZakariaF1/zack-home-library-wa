using HomeLibrary.Application.Imports;

namespace HomeLibrary.Application.Abstractions;

public interface IBookImportPublisher
{
    Task PublishAsync(BookImportMessage message, CancellationToken cancellationToken = default);
}
