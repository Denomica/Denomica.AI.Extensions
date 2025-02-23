using System;
using System.Collections.Generic;
using System.Text;

namespace Denomica.AI.Extensions.Configuration
{
    public class ModelDeploymentOptions
    {
        public string Endpoint { get; set; } = string.Empty;

        public string Key { get; set; } = string.Empty;

        public string? Name { get; set; }

        public int? Dimensions { get; set; } = null;

    }
}
