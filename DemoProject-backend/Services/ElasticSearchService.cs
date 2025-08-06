using DemoProject_backend.Models;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Nodes;
using System.Text.RegularExpressions;

using Elastic.Clients.Elasticsearch.QueryDsl;

using Task = System.Threading.Tasks.Task;

public class ElasticSearchService
{
    private readonly ElasticsearchClient _elasticClient;

    public ElasticSearchService(ElasticsearchClient elasticClient)
    {
        _elasticClient = elasticClient;
    }

    //public async Task IndexBusinessAsync(Business business)
    //{
    //    await _elasticClient.IndexDocumentAsync(business);
    //}
    public async Task<List<Business>> SearchBusinessesAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<Business>();

        var response = await _elasticClient.SearchAsync<Business>(s => s
            .Indices("search-demo")
            .Size(10)
            .Query(q => q
                .Term(t => t
                    .Field(f => f.Name.Suffix("keyword"))  // <-- exact, unanalyzed
                    .Value(searchTerm)
                )
            )
        );

        if (!response.IsValidResponse)
        {
            // (log or handle error)
            return new List<Business>();
        }

        return response.Documents.ToList();
    }




    public async Task BulkIndexBusinessesAsync(List<Business> businesses)
    {
        // Split into smaller batches (500-1000 documents per batch)
        var batchSize = 500;
        for (int i = 0; i < businesses.Count; i += batchSize)
        {
            var batch = businesses.Skip(i).Take(batchSize).ToList();

            var bulkResponse = await _elasticClient.BulkAsync(b => b
                .Index("search-demo")
                .IndexMany(batch)
                .Refresh(Refresh.WaitFor) // Only on last batch if needed
            );

            if (bulkResponse.Errors)
            {
                // Handle errors
            }
        }
    }


    public async Task<List<Business>> GetAllBusinessesForAdminAsync()
    {
        var searchResp = await _elasticClient.SearchAsync<Business>(s => s
            .Indices("businesses")
            .Query(q => q
                .Term(t => t.Field(f => f.IsActive).Value(true))
            )
        );

        var items = searchResp.Documents.ToList();
        return (items);
    }

}
