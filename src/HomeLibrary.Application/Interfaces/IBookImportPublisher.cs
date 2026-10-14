using HomeLibrary.Application.Imports;

namespace HomeLibrary.Application.Interfaces;

public interface IBookImportPublisher
{
    Task PublishAsync(BookImportMessage message, CancellationToken cancellationToken = default);
}
