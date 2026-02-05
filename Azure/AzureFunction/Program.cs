using AzureFunction.Client;
using AzureFunction.Config;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

// HTTP & ASP.NET features
builder.ConfigureFunctionsWebApplication();

// App insights
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Configuration
// bind settings defined in local.settings.json to the specified class
builder.Services.Configure<Settings>(builder.Configuration);
builder.Services.AddSingleton<IClientFactory, ClientFactory>();

// HttpClient factory (used by embedding / REST clients) - factory prevents duplicate http connections
// Local GPU embedding REST web API
builder.Services.AddHttpClient("GpuService", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Clients
builder.Services.AddSingleton<IBlobStorage>(sp =>
    sp.GetRequiredService<IClientFactory>().CreateBlobStorageClient());

builder.Services.AddSingleton<IEmbeddingClient>(sp =>
    sp.GetRequiredService<IClientFactory>().CreateGpuClient());

builder.Services.AddSingleton<IAiSearchClient>(sp =>
    sp.GetRequiredService<IClientFactory>().CreateAISearchClient());

builder.Services.AddSingleton<IChatCompletionClient>(sp =>
    sp.GetRequiredService<IClientFactory>().CreateChatClient());

builder.Build().Run();
