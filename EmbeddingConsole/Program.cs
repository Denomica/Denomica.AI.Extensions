using Denomica.AI.Extensions.Configuration;
using Denomica.AI.Extensions.Embedding;
using Denomica.AI.Extensions.Embeddings;
using Denomica.AI.Extensions.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using EmbeddingConsole.Properties;

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
    .AddSingleton<ITextEmbeddingGenerator, EmbeddingGenerator>()
;

var provider = services.BuildServiceProvider();
var embeddingGenerator = provider.GetRequiredService<ITextEmbeddingGenerator>();
var embeddings = await embeddingGenerator.GenerateAsync([Resources.MdDocument01]);
var count = embeddings.Count;

embeddings = await embeddingGenerator.GenerateAsync([""]);
count = embeddings.Count;

embeddings = await embeddingGenerator.GenerateAsync([null!]);
count = embeddings.Count;
//await Embed01(provider);

static async Task Embed01(ServiceProvider provider)
{
    var builder = await provider.GetRequiredService<EmbeddingBuilder>()
        .AddTextAsync("Hello world!");
    var embeddingsResult = await builder.BuildAsync();
    var embedding = embeddingsResult.Embedding;
}
