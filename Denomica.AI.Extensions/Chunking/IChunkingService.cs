using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Denomica.AI.Extensions.Chunking
{
    /// <summary>
    /// The interface implemented by a service that provides chunks for creating vector embeddings from.
    /// </summary>
    public interface IChunkingService
    {

        /// <summary>
        /// Returns a collection of streams containing chunks.
        /// </summary>
        IAsyncEnumerable<Stream> GetChunksAsync();

    }
}
