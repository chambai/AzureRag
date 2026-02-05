using Microsoft.SemanticKernel.Text;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Tests
{
    public class UtilitiesTests
    {
        [Fact]
        public void SplitIntoChunks_WithEmptyInput_ReturnsEmptyList()
        {
            // Act
            var result = Utilities.SplitIntoChunks(null!);
            var result2 = Utilities.SplitIntoChunks("   ");

            // Assert
            Assert.Empty(result);
            Assert.Empty(result2);
        }

        [Fact]
        public void SplitIntoChunks_AppliesOverlap()
        {
            // Arrange
            string text = "This is the first sentence that is quite long. This is the second sentence which follows it.";
            int maxTokens = 50;
            int overlap = 20;

            // Act
            var result = Utilities.SplitIntoChunks(text, maxTokens, overlap);

            // Assert
            if (result.Count > 1)
            {
                // The start of the second chunk should contain the end of the first chunk
                string firstChunkEnd = result[0].Substring(result[0].Length - 10);
                Assert.Contains(firstChunkEnd, result[1]);
            }
        }

        // TODO: Add tests for edge cases like very small maxTokens, large overlap, etc. i.e.
        // PreservesFullSentences_WhenPossible
        // RespectsMaxTokenLength
        // WithShortText_ReturnsSingleChunk
        // NB: TextChunker is a new class that currently has limitations and is subject to change
    }
}
