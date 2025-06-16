using Denomica.AI.Extensions.Configuration;
using Denomica.AI.Extensions.Text;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Denomica.AI.Extensions.Embedding
{
    public class EmbeddingGenerator : ITextEmbeddingGenerator
    {
        public EmbeddingGenerator(IOptions<EmbeddingModelDeploymentOptions> options, IHttpClientFactory httpFactory, IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.Options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            this.HttpClient = this.CreateHttpClient(this.Options, httpFactory);
        }

        public EmbeddingGenerator(EmbeddingModelDeploymentOptions options, IHttpClientFactory httpFactory, IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.Options = options ?? throw new ArgumentNullException(nameof(options));
            this.HttpClient = this.CreateHttpClient(this.Options, httpFactory);
        }

        private readonly EmbeddingModelDeploymentOptions Options;
        private readonly HttpClient HttpClient;
        private readonly IServiceProvider ServiceProvider;
        private readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(IEnumerable<string> values, EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default)
        {
            var chunker = this.ServiceProvider.GetService<IChunkingService>();
            var results = new List<GeneratedEmbeddings<Embedding<float>>>();
            foreach (var val in values)
            {
                if(null != chunker)
                {
                    using (var strm = new MemoryStream())
                    {
                        using (var writer = new StreamWriter(strm))
                        {
                            await writer.WriteAsync(val);
                            await writer.FlushAsync();
                            strm.Position = 0;

                            await foreach (var chunk in chunker.GetChunksAsync(strm))
                            {
                                var embedding = await this.GenerateEmbeddingAsync(chunk);
                                results.Add(embedding);
                            }
                        }
                    }
                }
                else
                {
                    var embedding = await this.GenerateEmbeddingAsync(val);
                    results.Add(embedding);
                }
            }

            var weightedEmbeddings = new List<WeightedEmbedding>();
            foreach (var result in results)
            {
                foreach (var data in result)
                {
                    var weightedEmbedding = new WeightedEmbedding
                    {
                        Embedding = data.Vector.ToArray(),
                        Weight = result?.Usage?.TotalTokenCount ?? 1
                    };
                    weightedEmbeddings.Add(weightedEmbedding);
                }
            }

            var combinedEmbedding = weightedEmbeddings.Combine();

            GeneratedEmbeddings<Embedding<float>> resultEmbedding = new GeneratedEmbeddings<Embedding<float>>();
            if (null != combinedEmbedding)
            {
                resultEmbedding = new GeneratedEmbeddings<Embedding<float>>
                {
                    AdditionalProperties = new AdditionalPropertiesDictionary(),

                    Usage = new UsageDetails
                    {
                        TotalTokenCount = combinedEmbedding.Weight
                    }
                };

                resultEmbedding.Add(new Embedding<float>(new ReadOnlyMemory<float>(combinedEmbedding.Embedding))
                {
                    AdditionalProperties = new AdditionalPropertiesDictionary(),
                    ModelId = this.Options.Name
                });

                resultEmbedding.AdditionalProperties.Add("modelName", this.Options.Name);
            }

            return resultEmbedding;
        }



        void IDisposable.Dispose()
        {

            this.HttpClient.Dispose();
        }

        object? IEmbeddingGenerator.GetService(Type serviceType, object? serviceKey)
        {
            return null;
        }


        private HttpClient CreateHttpClient(EmbeddingModelDeploymentOptions options, IHttpClientFactory factory)
        {
            var client = factory.CreateClient();
            if(options.Key?.Length > 0)
            {
                client.DefaultRequestHeaders.Add("api-key", options.Key);
            }

            var endpointUri = new Uri(options.Endpoint);
            client.BaseAddress = new Uri($"https://{endpointUri.Host}/models/embeddings");

            return client;
        }

        private async Task<GeneratedEmbeddings<Embedding<float>>> GenerateEmbeddingAsync(string input)
        {
            var requestMessage = new EmbeddingRequest
            {
                Model = this.Options?.Name ?? throw new NullReferenceException("Options.Name must not be null."),
                Input = new List<string>(new string[] { input }),
                Dimensions = this.Options.Dimensions
            };

            var payload = JsonSerializer.Serialize(requestMessage, this.SerializerOptions);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            var response = await this.HttpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responsePayload = await response.Content.ReadAsStringAsync();
            var embeddingResult = JsonSerializer.Deserialize<EmbeddingResponse>(responsePayload, this.SerializerOptions) ?? throw new NullReferenceException();

            var result = new GeneratedEmbeddings<Embedding<float>>()
            {
                AdditionalProperties = new AdditionalPropertiesDictionary()
            };

            foreach (var data in embeddingResult.Data)
            {
                result.Add(new Embedding<float>(new ReadOnlyMemory<float>(data.Embedding.ToArray())) { ModelId = requestMessage.Model });
            }

            result.Usage = new UsageDetails
            {
                TotalTokenCount = embeddingResult.Usage.Total_Tokens
            };

            return result;
        }
    }
}
