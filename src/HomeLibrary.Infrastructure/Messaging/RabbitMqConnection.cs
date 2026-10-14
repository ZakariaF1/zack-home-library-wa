using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace HomeLibrary.Infrastructure.Messaging;

public sealed class RabbitMqConnection : IAsyncDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqConnection> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private IConnection? _connection;

    public RabbitMqConnection(IOptions<RabbitMqOptions> options, ILogger<RabbitMqConnection> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IChannel> CreateChannelAsync(CancellationToken cancellationToken = default)
    {
        var connection = await GetConnectionAsync(cancellationToken);
        return await connection.CreateChannelAsync(cancellationToken: cancellationToken);
    }

    public async Task EnsureTopologyAsync(CancellationToken cancellationToken = default)
    {
        await using var channel = await CreateChannelAsync(cancellationToken);
        await channel.QueueDeclareAsync(
            queue: _options.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);
    }

    private async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection is { IsOpen: true })
        {
            return _connection;
        }

        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_connection is { IsOpen: true })
            {
                return _connection;
            }

            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password
            };

            Exception? lastError = null;
            for (var attempt = 1; attempt <= 10; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    _connection = await factory.CreateConnectionAsync("home-library", cancellationToken);
                    _logger.LogInformation(
                        "Connected to RabbitMQ at {Host}:{Port} (attempt {Attempt})",
                        _options.HostName,
                        _options.Port,
                        attempt);
                    return _connection;
                }
                catch (Exception ex)
                {
                    lastError = ex;
                    _logger.LogWarning(
                        ex,
                        "RabbitMQ connection attempt {Attempt} failed; retrying",
                        attempt);
                    await Task.Delay(TimeSpan.FromSeconds(Math.Min(attempt, 5)), cancellationToken);
                }
            }

            throw new InvalidOperationException("Unable to connect to RabbitMQ after retries.", lastError);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        _gate.Dispose();
    }
}
