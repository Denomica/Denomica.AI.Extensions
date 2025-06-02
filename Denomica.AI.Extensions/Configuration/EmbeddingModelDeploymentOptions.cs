using System;
using System.Collections.Generic;
using System.Text;

namespace Denomica.AI.Extensions.Configuration
{
    /// <summary>
    /// An options class for defining how to use a model deployment for embeddings in Azure AI Foundry.
    /// </summary>
    public class EmbeddingModelDeploymentOptions : ModelDeploymentOptions
    {
        /// <summary>
        /// The number of dimensions of the embeddings produced by the model.
        /// </summary>
        /// <remarks>
        /// The number of dimensions to use for the embedding. Note that not all embedding models
        /// support specifying the number of dimensions, and some models may have a minimum or
        /// maximum number of dimensions that can be used.
        /// </remarks>
        public int? Dimensions { get; set; } = null;
    }
}
