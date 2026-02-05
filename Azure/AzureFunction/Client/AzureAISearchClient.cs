using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;


namespace AzureFunction.Client
{
    internal class AzureAISearchClient : IAiSearchClient
    {
        private SearchClient _client;

        public AzureAISearchClient(SearchClient client)
        {
            _client = client;
        }

        public async Task StoreVectorAsync(
            float[] embeddingVector,
            string chunkText,
            string source,
            CancellationToken cancellationToken = default)
        {

            var chunk = new SearchDocumentChunk
            {
                Id = Guid.NewGuid().ToString(),
                Content = chunkText,
                Embedding = embeddingVector,
                Source = source
            };

            await _client.UploadDocumentsAsync(new[] { chunk });
        }

        public async Task<List<SearchDocumentChunk>> SearchByVectorAsync(
            float[] queryVector,
            int k,
            CancellationToken ct = default)

            // TODO: Have option of Hybrid Search (combining keywords + vectors) to get more accurate results
        {
            // Define the vector query parameters
            // Takes in the numeric vector of the search term and tells Azure to look 
            // for other vectors nearby in mathematical space
            var vectorQuery = new VectorizedQuery(queryVector)
            {
                KNearestNeighborsCount = k,         // return top k most similar matches
                Fields = { nameof(SearchDocumentChunk.Embedding) }    // Tells Azure which field in your index contains the vectors to compare against
            };

            // Run the search
            var searchOptions = new SearchOptions   // search options container
            {
                VectorSearch = new VectorSearchOptions  // vector related options container
                {
                    Queries = { vectorQuery }   // using vector query params defined above
                },
                // return the Id and Content only in the results (like an SQL SELECT)
                Select = { nameof(SearchDocumentChunk.Id), nameof(SearchDocumentChunk.Content) }    
            };

            // Map JSON results from the search into SearchDocumentChunk objects
            // null means "don't do a keyword search, ONLY do a vector search"
            SearchResults<SearchDocumentChunk> response = await _client.SearchAsync<SearchDocumentChunk>(null, searchOptions);

            // Process results
            var results = new List<SearchDocumentChunk>();
            // GetResultsAsync handles paging (if there are i.e. 1000 results, it  streams them in batches in a loop)
            await foreach (SearchResult<SearchDocumentChunk> result in response.GetResultsAsync())
            {
                results.Add(result.Document);
            }

            return results;
        }
    }

    public class SearchDocumentChunk
    {
        [SimpleField(IsKey = true)]
        public string Id { get; set; }

        [SearchableField]
        public string Content { get; set; }

        [VectorSearchField(VectorSearchDimensions = 768, VectorSearchProfileName = "mpnet-cosine-profile")]
        public float[] Embedding { get; set; }

        [SimpleField]
        public string Source { get; set; }

        // TODO: Programatically create the index for this class in Azure if it doesn't exist
    }

}
