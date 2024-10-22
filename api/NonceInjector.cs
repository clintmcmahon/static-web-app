using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;

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

            // Get the directory where the function is running
            string functionDirectory = Directory.GetCurrentDirectory();

            // Get all directories and files in that directory
            var directories = Directory.GetDirectories(functionDirectory);
            var files = Directory.GetFiles(functionDirectory);

            // Prepare a list of directories and files for display
            var result = "Directories:\n" + string.Join("\n", directories.Select(d => Path.GetFileName(d))) +
                         "\n\nFiles:\n" + string.Join("\n", files.Select(f => Path.GetFileName(f)));

            // Return the result as part of the HTTP response
            return new OkObjectResult(result);
        }
    }
}
