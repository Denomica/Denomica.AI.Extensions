using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denomica.AI.Extensions.Text
{
    /// <summary>
    /// A chunking service that breaks up text into chunks based on lines.
    /// </summary>
    public class LineChunkingService : ChunkingServiceBase
    {

        /// <inheritdoc/>
        protected override async Task<string?> GetNextChunkAsync(StreamReader chunkReader)
        {
            if (!chunkReader.EndOfStream)
            {
                return await chunkReader.ReadLineAsync();
            }

            return null;
        }
    }
}
