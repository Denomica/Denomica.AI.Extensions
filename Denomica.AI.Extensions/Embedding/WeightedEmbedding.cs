using System;
using System.Collections.Generic;
using System.Text;

namespace Denomica.AI.Extensions.Embedding
{
    internal class WeightedEmbedding
    {
        public float[] Embedding { get; set; } = new float[0];

        public long Weight { get; set; }
    }
}
