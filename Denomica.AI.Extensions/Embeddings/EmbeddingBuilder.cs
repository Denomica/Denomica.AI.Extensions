using Azure.AI.Inference;
using Denomica.AI.Extensions.Chunking;
using Denomica.AI.Extensions.Configuration;
using Denomica.AI.Extensions.Messages;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Denomica.AI.Extensions.Embeddings
{
    /// <summary>
    /// A builder for creating embeddings from text chunks.
    /// </summary>
    public class EmbeddingBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddingBuilder"/> class.
        /// </summary>
        /// <param name="optionsFactory">The options factory that the builder uses to get its options.</param>
        /// <param name="httpClient">The HTTP client used by the builder to communicate with the embedding model in Azure AI Foundry.</param>
        public EmbeddingBuilder(IOptionsFactory<ModelDeploymentOptions> optionsFactory)
        {
            this.Options = optionsFactory.Create(EmbeddingBuilder.OptionsKey);

            this.EmbeddingsClient = this.CreateClient(this.Options);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddingBuilder"/> class.
        /// </summary>
        /// <param name="options">The options used by the builder instance.</param>
        /// <param name="httpClient">The HTTP client used by the builder to communicate with the embedding model in Azure AI Foundry.</param>
        public EmbeddingBuilder(ModelDeploymentOptions options)
        {
            this.Options = options;

            this.EmbeddingsClient = this.CreateClient(this.Options);
        }



        /// <summary>
        /// The key to use for naming the options for the builder.
        /// </summary>
        /// <remarks>
        /// This key must be used when you add the options for this builder to your service collection.
        /// </remarks>
        public const string OptionsKey = "embedding-builder-options";

        private ModelDeploymentOptions Options { get; }
        private JsonSerializerOptions SerializationOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private EmbeddingsClient EmbeddingsClient;

        private List<Task<string>> Chunks = new List<Task<string>>();
        /// <summary>
        /// Adds a text chunk to the builder.
        /// </summary>
        /// <remarks>
        /// An embedding is generated for each chunk separately. The embeddings are then combined into a single embedding by averaging the 
        /// embeddings of the individual chunks. The average is weighted by using the number of tokens consumed for each chunk as the weight.
        /// This way, a chunk that consumes more tokens will have a greater impact on the final embedding.
        /// </remarks>
        /// <param name="text">The text chunk to add to the builder.</param>
        /// <returns>The builder instance.</returns>
        public EmbeddingBuilder AddTextChunk(string text)
        {
            this.Chunks.Add(Task.FromResult(text));
            return this;
        }

        /// <summary>
        /// Adds a task that produces a text chunk to the builder.
        /// </summary>
        /// <param name="text">A task that produces a text to use as chunk.</param>
        /// <returns>The builder instance.</returns>
        public EmbeddingBuilder AddTextChunk(Task<string> text)
        {
            this.Chunks.Add(text);
            return this;
        }

        /// <summary>
        /// Adds each chunk returned by <paramref name="chunkingService"/> as a text chunk to the current builder.
        /// </summary>
        /// <param name="chunkingService">The chunking service to use to produce the chunks.</param>
        public async Task<EmbeddingBuilder> AddTextChunksAsync(IChunkingService chunkingService)
        {
            await foreach(var chunk in chunkingService.GetChunksAsync())
            {
                if(null != chunk)
                {
                    using (var reader = new StreamReader(chunk))
                    {
                        this.AddTextChunk(reader.ReadToEndAsync());
                    }
                }
                else
                {
                    break;
                }
            }

            return this;
        }

        /// <summary>
        /// Clears all chunks from the builder.
        /// </summary>
        /// <returns>The builder instance.</returns>
        public EmbeddingBuilder ClearChunks()
        {
            this.Chunks.Clear();
            return this;
        }

        /// <summary>
        /// Builds the embeddings for the chunks added to the builder.
        /// </summary>
        /// <returns>
        /// Returns a single embedding that is the average of the embeddings of the individual chunks.
        /// The average is weighted by the number of tokens consumed for each chunk.
        /// </returns>
        /// <remarks>
        /// This method clears the chunks after building the embedding, so you can call it multiple 
        /// times to build embeddings from different sets of chunks.
        /// </remarks>
        public async Task<EmbeddingBuildResult> BuildAsync()
        {
            var results = new List<EmbeddingsResult>();

            foreach(var chunk in this.Chunks)
            {
                var embedding = await this.GenerateEmbeddingAsync(chunk);
                results.Add(embedding);
            }

            var result = this.CombineEmbeddingItems(results);
            result.Text = string.Join("", from x in this.Chunks select x.Result);
            result.Model = this.Options.Name ?? "";

            // We're done with the chunks, so clear them for the next embedding build.
            this.ClearChunks();

            return result;
        }



        private EmbeddingBuildResult CombineEmbeddingItems(IEnumerable<EmbeddingsResult> items)
        {
            var weightedEmbeddings = items
                .SelectMany(e => e.Data.Select(d => new WeightedEmbedding { Embedding = d.Embedding.ToObjectFromJson<float[]>() ?? new float[0], Weight = e.Usage.TotalTokens }))
                .ToList();

            var firstEmbeddingLength = weightedEmbeddings.First().Embedding.Length;
            var resultEmbedding = new float[firstEmbeddingLength];
            foreach (var we in weightedEmbeddings)
            {
                for (int i = 0; i < firstEmbeddingLength; i++)
                {
                    resultEmbedding[i] += we.Embedding[i] * we.Weight;
                }
            }

            var totalWeight = weightedEmbeddings.Sum(we => we.Weight);
            for (int i = 0; i < resultEmbedding.Length; i++)
            {
                resultEmbedding[i] /= totalWeight;
            }

            return new EmbeddingBuildResult
            {
                Embedding = resultEmbedding,
                TokensConsumed = totalWeight
            };
        }

        private EmbeddingsClient CreateClient(ModelDeploymentOptions options)
        {
            return new EmbeddingsClient(new Uri(this.Options.Endpoint), new Azure.AzureKeyCredential(this.Options.Key));
        }

        private async Task<EmbeddingsResult> GenerateEmbeddingAsync(Task<string> input)
        {
            var options = new EmbeddingsOptions(new string[] { await input })
            {
                Model = this.Options.Name,
                Dimensions = this.Options.Dimensions
            };

            var result = await this.EmbeddingsClient.EmbedAsync(options);
            return result.Value;
        }




        private class WeightedEmbedding
        {
            public float[] Embedding { get; set; } = new float[0];

            public int Weight { get; set; }
        }
    }
}
