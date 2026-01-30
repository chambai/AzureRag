using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class Utilities
{
    public static List<string> SplitIntoChunks(string text, int maxTokens = 700, int overlap = 100)
    {
        // Simple paragraph split
        var paragraphs = Regex.Split(text, @"\r?\n\r?\n");
        var chunks = new List<string>();
        var currentChunk = "";

        foreach (var para in paragraphs)
        {
            if ((currentChunk + para).Length > maxTokens)
            {
                chunks.Add(currentChunk);
                currentChunk = para.Substring(0, Math.Min(para.Length, maxTokens));
            }
            else
            {
                currentChunk += " " + para;
            }
        }
        if (!string.IsNullOrEmpty(currentChunk))
            chunks.Add(currentChunk);

        return chunks;
    }
}
