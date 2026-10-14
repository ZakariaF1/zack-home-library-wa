using HomeLibrary.Infrastructure;
using HomeLibrary.Infrastructure.Messaging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHomeLibraryInfrastructure(builder.Configuration);
builder.Services.AddHostedService<BookImportConsumer>();

var host = builder.Build();
await host.Services.InitializeHomeLibraryAsync();
await host.RunAsync();
