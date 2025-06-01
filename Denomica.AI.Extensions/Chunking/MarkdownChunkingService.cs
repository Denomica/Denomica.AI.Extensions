using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Denomica.AI.Extensions.Chunking
{
    public class MarkdownChunkingService : ChunkingServiceBase
    {

        protected override async Task<string?> GetNextChunkAsync(StreamReader chunkReader)
        {
            return null;
        }
    }
}
