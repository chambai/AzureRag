using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.SemanticKernel.Text;

public static class Utilities
{
    public static List<string> SplitIntoChunks(string text, int maxTokens = 700, int overlap = 100)
    {
        if (string.IsNullOrWhiteSpace(text)) return new List<string>();

        // Safety: overlap must be less than maxTokens or SK will throw ArgumentException
        int safeOverlap = Math.Min(overlap, maxTokens - 1);
        if (safeOverlap < 0) safeOverlap = 0;

#pragma warning disable SKEXP0050 // this is a warning on TextChunker that it is subject to change 
        var lines = TextChunker.SplitPlainTextLines(text, maxTokens);

        // Use safeOverlap here
        var chunks = TextChunker.SplitPlainTextParagraphs(lines, maxTokens, safeOverlap);
#pragma warning restore SKEXP0050

        return chunks;
    }
}
