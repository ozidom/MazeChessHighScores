using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Worker;
using System.Net;

namespace Mazechess.Function
{
    public class HttpScores
    {
        private readonly ILogger<HttpScores> _logger;
        private static readonly string cosmosDbEndpoint = System.Environment.GetEnvironmentVariable("COSMOS_DB_ENDPOINT");
        private static readonly string cosmosDbKey = System.Environment.GetEnvironmentVariable("COSMOS_DB_KEY");
        private readonly string databaseId = "MazechessScoreDB";       
        private readonly string containerId = "scores";

        private CosmosClient cosmosClient;

        public HttpScores(ILogger<HttpScores> logger)
        {
            _logger = logger;
            cosmosClient = new CosmosClient(cosmosDbEndpoint, cosmosDbKey);
        }

        [Function("HttpScores")]
        public async Task<IActionResult> Run([Microsoft.Azure.Functions.Worker.HttpTrigger(Microsoft.Azure.Functions.Worker.AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
          
            try
            {
                var container = cosmosClient.GetContainer(databaseId, containerId);

                string queryString = "SELECT * FROM c ORDER BY c.moves ASC";
                QueryDefinition query = new QueryDefinition(queryString);
                FeedIterator<dynamic> resultSet = container.GetItemQueryIterator<dynamic>(query);

                List<dynamic> highScores = new List<dynamic>();

                while (resultSet.HasMoreResults)
                {
                    FeedResponse<dynamic> response = await resultSet.ReadNextAsync(); // Await here requires async method
                    foreach (var item in response)
                    {
                        highScores.Add(item);
                    }
                }

                return new OkObjectResult(JsonConvert.SerializeObject(highScores));
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"CosmosDB query failed with error: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }
    }
}
