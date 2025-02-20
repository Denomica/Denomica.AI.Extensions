using System;
using System.Collections.Generic;
using System.Text;

namespace Denomica.AI.Messages
{
    public class EmbeddingRequest
    {

        public string Input { get; set; } = string.Empty;

        public string? Model { get; set; }

    }
}
