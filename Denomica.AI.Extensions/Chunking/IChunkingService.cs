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
        /// Sets or returns the maximum chunk length returned by the chunking service.
        /// </summary>
        int MaxChunkLength { get; set; }

        /// <summary>
        /// Returns a collection of strings representing
        /// </summary>
        /// <param name="input">A stream containing the input string to chunk up.</param>
        IAsyncEnumerable<string> GetChunksAsync(Stream input);

    }
}
