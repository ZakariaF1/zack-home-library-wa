using System.Text;
using System.Text.Json;
using HomeLibrary.Application.Imports;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace HomeLibrary.Infrastructure.Messaging;

public sealed class BookImportConsumer(
    RabbitMqConnection connection,
    IOptions<RabbitMqOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<BookImportConsumer> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await connection.EnsureTopologyAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConsumeLoopAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Book import consumer disconnected; retrying");
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }
        }
    }

    private async Task ConsumeLoopAsync(CancellationToken stoppingToken)
    {
        await using var channel = await connection.CreateChannelAsync(stoppingToken);
        await channel.BasicQosAsync(0, 10, false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.ToArray());
                var message = JsonSerializer.Deserialize<BookImportMessage>(json, JsonOptions);
                if (message is null)
                {
                    logger.LogWarning("Ignoring invalid book import payload");
                    await channel.BasicAckAsync(args.DeliveryTag, false, stoppingToken);
                    return;
                }

                await using var scope = scopeFactory.CreateAsyncScope();
                var persist = scope.ServiceProvider.GetRequiredService<PersistImportedBook>();
                await persist.ExecuteAsync(message, stoppingToken);
                await channel.BasicAckAsync(args.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process book import message");
                await channel.BasicNackAsync(args.DeliveryTag, false, requeue: true, stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue: options.Value.QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        logger.LogInformation("Listening for book imports on queue {Queue}", options.Value.QueueName);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
