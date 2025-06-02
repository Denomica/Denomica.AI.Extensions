using Denomica.AI.Extensions.Chunking;
using Denomica.AI.Extensions.Configuration;
using Denomica.AI.Extensions.Embeddings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

IConfigurationRoot config = new ConfigurationBuilder()
    .AddJsonFile("local.settings.json", false)
    .Build();

var services = new ServiceCollection()
    .AddSingleton(config)
    .AddSingleton<IConfiguration>(config)

    .AddHttpClient()

    .AddOptions<EmbeddingModelDeploymentOptions>()
    .Configure<IConfigurationRoot>((opt, root) =>
    {
        root.GetSection("embedding:model").Bind(opt);
    }).Services
    
    .AddSingleton<IChunkingService, LineChunkingService>()
    .AddTransient<EmbeddingBuilder>(sp =>
    {
        var chunker = sp.GetRequiredService<IChunkingService>();
        var options = sp.GetRequiredService<IOptions<EmbeddingModelDeploymentOptions>>();
        return new EmbeddingBuilder(options, chunker);
    })
;

var provider = services.BuildServiceProvider();
var builder = await provider.GetRequiredService<EmbeddingBuilder>()
    .AddTextAsync("Hello world!");
var embeddingsResult = await builder.BuildAsync();

var embedding = embeddingsResult.Embedding;
