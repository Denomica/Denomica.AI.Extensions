using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Denomica.AI.Extensions.Chunking
{
    /// <summary>
    /// A base class for chunking services that provides a default implementation of the <see cref="IChunkingService"/> interface.
    /// </summary>
    public abstract class ChunkingServiceBase : IChunkingService
    {
        /// <inheritdoc/>
        public virtual int MaxChunkLength { get; set; } = 25000;

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<string> GetChunksAsync(Stream input)
        {
            using(var reader = new StreamReader(input, Encoding.UTF8, true, 4096, true))
            {
                var chunkBuilder = new StringBuilder();
                string? nextChunk = null;
                do
                {
                    nextChunk = await this.GetNextChunkAsync(reader);
                    if (null != nextChunk)
                    {
                        if (chunkBuilder.Length + nextChunk.Length <= this.MaxChunkLength)
                        {
                            chunkBuilder.Append(nextChunk);
                        }
                        else
                        {
                            yield return chunkBuilder.ToString();
                            chunkBuilder.Clear();
                        }
                    }
                }
                while (null != nextChunk);

                if (chunkBuilder.Length > 0)
                {
                    yield return chunkBuilder.ToString();
                    chunkBuilder.Clear();
                }
            }

            yield break;
        }

        /// <summary>
        /// Returns a collection of strings representing chunks of the input string.
        /// </summary>
        /// <param name="input">The input string to chunk up.</param>
        public async IAsyncEnumerable<string> GetChunksAsync(string input)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            {
                await foreach (var chunk in this.GetChunksAsync(stream))
                {
                    yield return chunk;
                }
            }
        }



        protected abstract Task<string?> GetNextChunkAsync(StreamReader chunkReader);
    }
}
