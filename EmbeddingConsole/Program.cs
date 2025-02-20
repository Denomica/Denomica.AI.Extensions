using Denomica.AI.Configuration;
using Denomica.AI.Embeddings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

IConfigurationRoot config = new ConfigurationBuilder()
    .AddJsonFile("local.settings.json", false)
    .Build();

var services = new ServiceCollection()
    .AddSingleton(config)
    .AddSingleton<IConfiguration>(config)

    .AddHttpClient()

    .AddOptions<ModelDeploymentOptions>(EmbeddingBuilder.OptionsKey)
    .Configure<IConfigurationRoot>((opt, root) =>
    {
        root.GetSection("embedding:model").Bind(opt);
    }).Services
    .AddSingleton<EmbeddingBuilder>();
;
var provider = services.BuildServiceProvider();
var builder = provider.GetRequiredService<EmbeddingBuilder>();

var embeddingsResult = await builder
    .ClearInputs()
    .AppendTextInput("Hello, world!")
    .BuildAsync();

var embedding = embeddingsResult.Data[0].Embedding;
