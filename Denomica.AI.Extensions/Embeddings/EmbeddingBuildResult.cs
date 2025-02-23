using System;
using System.Collections.Generic;
using System.Text;

namespace Denomica.AI.Extensions.Embeddings
{
    /// <summary>
    /// The result of building an embedding from one or more text chunks.
    /// </summary>
    /// <remarks>
    /// If you build an embedding from multiple chunks, the embedding is the average of the embeddings 
    /// of the individual chunks. The average is weighted by the number of tokens consumed for each chunk.
    /// </remarks>
    public class EmbeddingBuildResult
    {
        /// <summary>
        /// The complete text that was used to generate the embedding.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The embedding.
        /// </summary>
        public float[] Embedding { get; set; }

        /// <summary>
        /// The number of tokens consumed to generate the embedding.
        /// </summary>
        public int TokensConsumed { get; set; }

        /// <summary>
        /// The name of the embedding model that was used to build the result.
        /// </summary>
        public string Model { get; set; }
    }
}
