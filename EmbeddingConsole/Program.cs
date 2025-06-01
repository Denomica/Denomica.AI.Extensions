using Denomica.AI.Extensions.Chunking;
using Denomica.AI.Extensions.Configuration;
using Denomica.AI.Extensions.Embeddings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

IConfigurationRoot config = new ConfigurationBuilder()
    .AddJsonFile("local.settings.json", false)
    .Build();

var services = new ServiceCollection()
    .AddSingleton(config)
    .AddSingleton<IConfiguration>(config)

    .AddHttpClient()

    .AddOptions<ModelDeploymentOptions>()
    .Configure<IConfigurationRoot>((opt, root) =>
    {
        root.GetSection("embedding:model").Bind(opt);
    }).Services
    
    .AddTransient<EmbeddingBuilder>()
    .AddSingleton<IChunkingService, LineChunkingService>()
;

var provider = services.BuildServiceProvider();
var builder = await provider.GetRequiredService<EmbeddingBuilder>()
    .AddTextChunksAsync("Hello world!", provider.GetRequiredService<IChunkingService>());
var embeddingsResult = await builder.BuildAsync();

var embedding = embeddingsResult.Embedding;
