# Denomica.AI.Extensions

Denomica.AI.Extensions is a library that supports and extends AI services and capabilities in Azure.

## Version Highlights

The main hihglights in the published versions are outlined below.

### v1.0.0-beta.8

- Create a new `EmbeddingGenerator` class that implements the [`IEmbeddingGenerator<TInput, TEmbedding>`](https://learn.microsoft.com/dotnet/api/microsoft.extensions.ai.iembeddinggenerator-2) interface defined in [`Microsoft.Extensions.AI`](https://www.nuget.org/packages/Microsoft.Extensions.AI). The `EmbeddingGenerator` class is defined in the `Denomica.AI.Extensions.Embedding` namespace.
- Marked the previous types defined in `Denomica.AI.Extensions.Embeddings` namespace as obsolete.

### v1.0.0-beta.7

- Downgraded all references Microsoft.Extensions.* packages to version 8.0.x in order to support Azure Functions applications running on in-process worker model on .NET 8.

### v1.0.0-beta.6

Preparing the library for a stable release. However, this library is still in beta, because it has a dependency on the `Azure.AI.Inference` package, which is still in preview. When that package is released, this library will be released as stable as well.

- Cleaned up old stuff from the project.
- Modified `EmbeddingBuilder` to use a configured `IChunkingService` implementation by default, and allow the use of a custom implementation if needed.
- Added a new `EmbeddingBuilderOptions` class to allow for more flexible configuration of the embedding builder.

### v1.0.0-beta.5

- Fixed a problem with the `ChunkingServiceBase` class.

### v1.0.0-beta.4

- Added an overloaded method to the `EmbeddingBuilder` implementation.

### v1.0.0-beta.3

- Modified the `IChunkingService` interface, and added the `LineChunkingService` implementation of that interface.

### v1.0.0-beta.2

- Simplified how the `EmbeddingBuilder` class uses options specified in `ModelDeploymentOptions`.

### v1.0.0-beta.1

- Modified the `EmbeddingBuilder` class to support the use of Azure AI Foundry's embedding models by using the [`Azure.AI.Inference`](https://www.nuget.org/packages/Azure.AI.Inference) package.

### v1.0.0-alpha.2

- Added support for configuring the embedding model name in the config file.

### v1.0.0-alpha.1

- Initial release of the library.
- The first version of the EmbeddingBuilder class that supports creating vector embeddings for text data using embedding models in Azure AI Foundry.