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
builder.Services.AddSingleton<IConfig, AppConfig>();

// HttpClient factory (used by embedding / REST clients)
builder.Services.AddHttpClient();

// Clients
builder.Services.AddSingleton<IBlobStorage>(new AzureBlobStorage(Factory.CreateBlobClient()));
builder.Services.AddSingleton<IEmbeddingClient, GpuEmbeddingClient>();
builder.Services.AddSingleton<ISearchClient, AzureSearchClient>();
builder.Services.AddSingleton<IChatCompletionClient, AzureOpenAIChatClient>();

builder.Build().Run();
