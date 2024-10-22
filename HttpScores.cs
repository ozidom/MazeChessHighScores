using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Mazechess.Function
{
    public class HttpScores
    {
        private readonly ILogger<HttpScores> _logger;

        public HttpScores(ILogger<HttpScores> logger)
        {
            _logger = logger;
        }

        [Function("HttpScores")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
