using Denomica.AI.Configuration;
using Denomica.AI.Messages;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Denomica.AI.Embeddings
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
        public EmbeddingBuilder(IOptionsFactory<ModelDeploymentOptions> optionsFactory, HttpClient httpClient)
        {
            this.Options = optionsFactory.Create(EmbeddingBuilder.OptionsKey);
            this.HttpClient = httpClient;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddingBuilder"/> class.
        /// </summary>
        /// <param name="options">The options used by the builder instance.</param>
        /// <param name="httpClient">The HTTP client used by the builder to communicate with the embedding model in Azure AI Foundry.</param>
        public EmbeddingBuilder(ModelDeploymentOptions options, HttpClient httpClient)
        {
            this.Options = options;
            this.HttpClient = httpClient;
        }

        /// <summary>
        /// The key to use for naming the options for the builder.
        /// </summary>
        /// <remarks>
        /// This key must be used when you add the options for this builder to your service collection.
        /// </remarks>
        public const string OptionsKey = "embedding-builder-options";

        private ModelDeploymentOptions Options { get; }
        private JsonSerializerOptions SerializationOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        private HttpClient HttpClient { get; }

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
        public async Task<EmbeddingBuildResult> BuildAsync()
        {
            var textBuilder = new StringBuilder();
            var embeddings = new List<EmbeddingResponse>();
            foreach (var ip in this.Chunks)
            {
                embeddings.Add(await this.GenerateEmbeddingAsync(ip));
                textBuilder.Append(ip.Result);
            }

            var result = this.CombineEmbeddingResponses(embeddings);
            result.Text = textBuilder.ToString();

            return result;
        }



        private EmbeddingBuildResult CombineEmbeddingResponses(IEnumerable<EmbeddingResponse> embeddings)
        {
            var weightedEmbeddings = embeddings
                .SelectMany(e => e.Data.Select(d => new WeightedEmbedding { Embedding = d.Embedding, Weight = e.Usage.Total_Tokens }))
                .ToList();

            if (!weightedEmbeddings.Any())
            {
                throw new ArgumentException("Embeddings must not be an empty collection.", nameof(embeddings));
            }

            var firstEmbeddingLength = weightedEmbeddings.First().Embedding.Length;
            if (weightedEmbeddings.Any(we => we.Embedding.Length != firstEmbeddingLength))
            {
                throw new ArgumentException("All embeddings must have the same length.", nameof(embeddings));
            }

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
                TokensConsumed = totalWeight,
                Model = embeddings.First().Model
            };
        }

        private HttpRequestMessage CreateRequest(EmbeddingRequest payload)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, this.Options.Endpoint);
            req.Headers.Add("api-key", this.Options.Key);
            var jsonPayload = JsonSerializer.Serialize(payload, this.SerializationOptions);
            req.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            return req;
        }

        private async Task<EmbeddingResponse> GenerateEmbeddingAsync(Task<string> input)
        {
            var payload = new EmbeddingRequest { Input = await input };
            var request = this.CreateRequest(payload);
            var response = await this.HttpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                using (var responseStream = await response.Content.ReadAsStreamAsync())
                {
                    return await JsonSerializer.DeserializeAsync<EmbeddingResponse>(responseStream, this.SerializationOptions) ?? new EmbeddingResponse();
                }
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var json = await response.Content.ReadAsStringAsync();
                throw new Exception($"Response BadRequest (400) from endpoint {this.Options.Endpoint}. {json}");
            }
            else
            {
                throw new Exception($"Unsuccessful status code {response.StatusCode} from endpoint {this.Options.Endpoint}");
            }
        }

        private class WeightedEmbedding
        {
            public float[] Embedding { get; set; } = new float[0];

            public int Weight { get; set; }
        }
    }
}
