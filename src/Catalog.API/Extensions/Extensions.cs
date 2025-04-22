﻿using eShop.Catalog.API.Services;
using Microsoft.Extensions.AI;
using OllamaSharp;

public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        // Avoid loading full database config and migrations if startup
        // is being invoked from build-time OpenAPI generation
        if (builder.Environment.IsBuild())
        {
            builder.Services.AddDbContext<CatalogContext>();
            return;
        }

        builder.Services.AddDbContextPool<CatalogContext>(dbContextOptionsBuilder =>
        {
            var connectionString = builder.Configuration.GetConnectionString("CatalogDb");

            dbContextOptionsBuilder.UseNpgsql(connectionString, builder =>
            {
                builder.UseVector();
            });
        });

        // REVIEW: This is done for development ease but shouldn't be here in production
        builder.Services.AddMigration<CatalogContext, CatalogContextSeed>();

        // Add the integration services that consume the DbContext
        builder.Services.AddTransient<IIntegrationEventLogService, IntegrationEventLogService<CatalogContext>>();

        builder.Services.AddTransient<ICatalogIntegrationEventService, CatalogIntegrationEventService>();

        builder.AddRabbitMqEventBus("eventbus")
               .AddSubscription<OrderStatusChangedToAwaitingValidationIntegrationEvent, OrderStatusChangedToAwaitingValidationIntegrationEventHandler>()
               .AddSubscription<OrderStatusChangedToPaidIntegrationEvent, OrderStatusChangedToPaidIntegrationEventHandler>();

        builder.Services.AddOptions<CatalogOptions>()
            .BindConfiguration(nameof(CatalogOptions));

        builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(_ =>
        {
            var ollama = new OllamaApiClient(builder.Configuration["Ollama:Endpoint"], builder.Configuration["Ollama:EmbeddingModel"]);
            return ollama;
        });

        builder.Services.AddScoped<ICatalogAI, CatalogAI>();
    }
}
