using System.Text;
using System.Text.Json;
using HomeLibrary.Application.Abstractions;
using HomeLibrary.Application.Imports;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace HomeLibrary.Infrastructure.Messaging;

public sealed class RabbitMqBookImportPublisher(
    RabbitMqConnection connection,
    IOptions<RabbitMqOptions> options) : IBookImportPublisher
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task PublishAsync(BookImportMessage message, CancellationToken cancellationToken = default)
    {
        await using var channel = await connection.CreateChannelAsync(cancellationToken);
        await channel.QueueDeclareAsync(
            queue: options.Value.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message, JsonOptions));
        var props = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent
        };

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: options.Value.QueueName,
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: cancellationToken);
    }
}
