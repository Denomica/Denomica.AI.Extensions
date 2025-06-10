using System;
using System.Collections.Generic;
using System.Text;

namespace Denomica.AI.Extensions.Embedding
{
    internal class EmbeddingRequest
    {
        public string Model { get; set; } = string.Empty;

        public List<string> Input { get; set; } = new List<string>();

        public int? Dimensions { get; set; }
    }
}
