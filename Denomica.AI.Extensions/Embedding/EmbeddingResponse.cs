using System;
using System.Collections.Generic;
using System.Text;

namespace Denomica.AI.Extensions.Embedding
{
    internal class EmbeddingResponse
    {
        public string Id { get; set; } = string.Empty;

        public string Object { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public List<EmbeddingData> Data { get; set; } = new List<EmbeddingData>();

        public EmbeddingUsage Usage { get; set; } = new EmbeddingUsage();
    }

    internal class EmbeddingData
    {
        public int Index { get; set; }

        public string Object { get; set; } = string.Empty;

        public List<float> Embedding { get; set; } = new List<float>();
    }

    internal class EmbeddingUsage
    {
        public int Prompt_Tokens { get; set; }

        public int Completion_Tokens { get; set; }

        public int Total_Tokens { get; set; }
    }
}
