
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Azure.AI.OpenAI;
using System.ClientModel;

var services = new ServiceCollection();

var aiClient = new AzureOpenAIClient(new Uri(""), new ApiKeyCredential(""), new AzureOpenAIClientOptions { });
//var chat = new AzureOpenAIChatCompletionService()