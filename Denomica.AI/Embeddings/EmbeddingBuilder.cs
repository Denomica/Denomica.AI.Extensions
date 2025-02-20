using Denomica.AI.Configuration;
using Denomica.AI.Messages;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Denomica.AI.Embeddings
{
    public class EmbeddingBuilder
    {

        public EmbeddingBuilder(IOptionsFactory<ModelDeploymentOptions> optionsFactory, HttpClient httpClient)
        {
            this.Options = optionsFactory.Create(EmbeddingBuilder.OptionsKey);
            this.HttpClient = httpClient;
        }

        public EmbeddingBuilder(ModelDeploymentOptions options, HttpClient httpClient)
        {
            this.Options = options;
            this.HttpClient = httpClient;
        }

        public const string OptionsKey = "embedding-builder-options";

        private ModelDeploymentOptions Options { get; }
        private JsonSerializerOptions SerializationOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        private HttpClient HttpClient { get; }

        private List<Task<string>> Inputs = new List<Task<string>>();
        public EmbeddingBuilder AppendTextInput(string text)
        {
            this.Inputs.Add(Task.FromResult(text));
            return this;
        }

        public EmbeddingBuilder ClearInputs()
        {
            this.Inputs.Clear();
            return this;
        }

        public async Task<EmbeddingResponse> BuildAsync()
        {
            var sb = new StringBuilder();
            foreach (var ip in this.Inputs)
            {
                sb.Append(await ip);
            }

            var requestPayload = new EmbeddingRequest { Input = sb.ToString() };
            var request = this.CreateRequest(requestPayload);

            var response = await this.HttpClient.SendAsync(request);
            if(response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<EmbeddingResponse>(json, this.SerializationOptions) ?? new EmbeddingResponse();
            }
            else if(response.StatusCode == HttpStatusCode.BadRequest)
            {
                var json = await response.Content.ReadAsStringAsync();
                throw new Exception($"Response BadRequest (400) from endpoint {this.Options.Endpoint}. {json}");
            }
            else
            {
                throw new Exception($"Unsuccessful status code {response.StatusCode} from endpoint {this.Options.Endpoint}");
            }
        }

        private HttpRequestMessage CreateRequest(EmbeddingRequest payload)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, this.Options.Endpoint);
            req.Headers.Add("api-key", this.Options.Key);
            var jsonPayload = JsonSerializer.Serialize(payload, this.SerializationOptions);
            req.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            return req;
        }
    }
}
