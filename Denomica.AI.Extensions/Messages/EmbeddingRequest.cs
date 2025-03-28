﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Denomica.AI.Extensions.Messages
{
    public class EmbeddingRequest
    {

        public string Input { get; set; } = string.Empty;

        public string? Model { get; set; } = null;

        public int? Dimensions { get; set; } = null;
    }
}
