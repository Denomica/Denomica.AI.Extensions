using System;
using System.Collections.Generic;
using System.Text;

namespace Denomica.AI.Embeddings
{
    public class EmbeddingResponse
    {
        public string Object { get; set; }

        public EmbeddingResponseData[] Data { get; set; }

        public string Model { get; set; }

        public EmbeddingResponseUsage Usage { get; set; }
    }

    public class EmbeddingResponseData
    {
        public string Object { get; set; }

        public int Index { get; set; }

        public float[] Embedding { get; set; }
    }

    public class EmbeddingResponseUsage
    {
        public int Prompt_Tokens { get; set; }

        public int Total_Tokens { get; set; }
    }
}
