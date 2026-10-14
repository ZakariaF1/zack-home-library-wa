using HomeLibrary.Application.Abstractions;
using HomeLibrary.Application.Books;
using HomeLibrary.Application.Imports;
using HomeLibrary.Infrastructure.Csv;
using HomeLibrary.Infrastructure.Messaging;
using HomeLibrary.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeLibrary.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddHomeLibraryInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));

        var connectionString = configuration.GetConnectionString("Library")
            ?? throw new InvalidOperationException("Connection string 'Library' is required.");

        services.AddDbContext<LibraryDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddSingleton<RabbitMqConnection>();
        services.AddScoped<IBookRepository, EfBookRepository>();
        services.AddSingleton<IBookImportPublisher, RabbitMqBookImportPublisher>();
        services.AddSingleton<ICsvBookParser, CsvBookParser>();

        services.AddScoped<QueueBookImport>();
        services.AddScoped<ListBooks>();

        return services;
    }

    public static async Task InitializeHomeLibraryAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

        Exception? lastError = null;
        for (var attempt = 1; attempt <= 10; attempt++)
        {
            try
            {
                await db.Database.MigrateAsync(cancellationToken);
                lastError = null;
                break;
            }
            catch (Exception ex)
            {
                lastError = ex;
                await Task.Delay(TimeSpan.FromSeconds(Math.Min(attempt, 5)), cancellationToken);
            }
        }

        if (lastError is not null)
        {
            throw new InvalidOperationException("Unable to migrate the library database after retries.", lastError);
        }

        var rabbit = scope.ServiceProvider.GetRequiredService<RabbitMqConnection>();
        await rabbit.EnsureTopologyAsync(cancellationToken);
    }
}
