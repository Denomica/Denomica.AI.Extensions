using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Denomica.AI.Extensions.Embedding
{
    public interface ITextEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
    {
    }
}
