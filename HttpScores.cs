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

            if (req.Method == HttpMethods.Get)
            {
                // Handle GET request: return the high scores
                return await GetHighScores();
            }
            else if (req.Method == HttpMethods.Post)
            {
                // Handle POST request: add a new high score
                return await AddHighScore(req);
            }

            return new BadRequestResult();
        }


       [Function("Ping")]
        public IActionResult Ping(
         [Microsoft.Azure.Functions.Worker.HttpTrigger(Microsoft.Azure.Functions.Worker.AuthorizationLevel.Anonymous, "get", Route = "ping")] HttpRequest req)
            {
                _logger.LogInformation("Ping endpoint was called.");
                return new OkObjectResult(new { message = "pong" });
            }

        private async Task<IActionResult> GetHighScores()
        {
            try
            {
                var container = cosmosClient.GetContainer(databaseId, containerId);

                  string todayDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

                string queryString = $"SELECT * FROM c WHERE STARTSWITH(c.timestamp, '{todayDate}') ORDER BY c.moves, c.time ASC OFFSET 0 LIMIT 10";

                QueryDefinition query = new QueryDefinition(queryString);
                FeedIterator<dynamic> resultSet = container.GetItemQueryIterator<dynamic>(query);

                List<dynamic> highScores = new List<dynamic>();

                while (resultSet.HasMoreResults)
                {
                    FeedResponse<dynamic> response = await resultSet.ReadNextAsync();
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

        private async Task<IActionResult> GetYesterdaysHighScores()
{
    try
    {
        var container = cosmosClient.GetContainer(databaseId, containerId);

        // Get yesterday's date in UTC
        string yesterdayDate = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");

        // Adjust the query to match yesterday's scores
        string queryString = $"SELECT * FROM c WHERE STARTSWITH(c.timestamp, '{yesterdayDate}') ORDER BY c.moves, c.time ASC OFFSET 0 LIMIT 10";

        QueryDefinition query = new QueryDefinition(queryString);
        FeedIterator<dynamic> resultSet = container.GetItemQueryIterator<dynamic>(query);

        List<dynamic> highScores = new List<dynamic>();

        while (resultSet.HasMoreResults)
        {
            FeedResponse<dynamic> response = await resultSet.ReadNextAsync();
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

        private async Task<IActionResult> AddHighScore(HttpRequest req)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var newScore = JsonConvert.DeserializeObject<dynamic>(requestBody);

                var container = cosmosClient.GetContainer(databaseId, containerId);

                // Create a new score entry
                var result = await container.CreateItemAsync(newScore);

                // Return a success message with the inserted data
                return new OkObjectResult(new { message = "High score submitted successfully", score = result.Resource });
            }
            catch (CosmosException ex)
            {
                //_logger.LogError($"CosmosDB insertion failed with error: {ex.Message}");
                 return new OkObjectResult(new { message = ex.Message });
         
            }
        }
    }
}
