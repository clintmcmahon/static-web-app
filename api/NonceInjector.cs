using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace StaticWebApp.Test
{
    public class NonceInjector
    {
        private readonly ILogger<NonceInjector> _logger;

        public NonceInjector(ILogger<NonceInjector> logger)
        {
            _logger = logger;
        }

        [Function("NonceInjector")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
