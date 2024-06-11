using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ExtendedMemoryServerless
{
    private readonly string _openApiKey;
    private readonly List<InMemoryDocument> _documents;

    public ExtendedMemoryServerless(string openApiKey)
    {
        _openApiKey = openApiKey;
        _documents = new List<InMemoryDocument>();
    }

    public async Task ImportDocumentContentAsync(string content, string documentId)
    {
        var inMemoryDocument = new InMemoryDocument
        {
            Id = documentId,
            Content = content
        };

        _documents.Add(inMemoryDocument);
        await Task.CompletedTask; // Simulating async work
    }

    public Task<bool> IsDocumentReadyAsync(string documentId)
    {
        var documentExists = _documents.Any(doc => doc.Id == documentId);
        return Task.FromResult(documentExists);
    }

    public Task<MemoryAnswer> AskAsync(string input)
    {
        var matchingDocuments = _documents
            .Where(doc => doc.Content.Contains(input, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var answer = new MemoryAnswer
        {
            Result = matchingDocuments.Any()
                ? string.Join("\n", matchingDocuments.Select(doc => doc.Content))
                : "No relevant documents found.",
            RelevantSources = matchingDocuments
                .Select(doc => new MemorySource
                {
                    SourceName = doc.Id,
                    Link = "http://example.com", // Placeholder link
                    Partitions = new List<MemoryPartition> { new MemoryPartition { LastUpdate = DateTime.Now } }
                }).ToList()
        };

        Console.WriteLine($"Found {matchingDocuments.Count} matching documents for query '{input}'");

        return Task.FromResult(answer);
    }


    private class InMemoryDocument
    {
        public string Id { get; set; }
        public string Content { get; set; }
    }
}

public class MemoryAnswer
{
    public string Result { get; set; }
    public List<MemorySource> RelevantSources { get; set; }
}

public class MemorySource
{
    public string SourceName { get; set; }
    public string Link { get; set; }
    public List<MemoryPartition> Partitions { get; set; }
}

public class MemoryPartition
{
    public DateTime LastUpdate { get; set; }
}
