using Denomica.AI.Extensions.Embeddings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Denomica.AI.Extensions.Embedding
{
    public static class EmbeddingExtensionMethods
    {

        internal static WeightedEmbedding Combine(this IEnumerable<WeightedEmbedding> weightedEmbeddings)
        {
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

            return new WeightedEmbedding
            {
                Embedding = resultEmbedding,
                Weight = totalWeight
            };
        }
    }
}
