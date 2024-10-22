using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

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
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            // Define the path to the index.html in the "angular-basic" folder
            string functionDirectory = Directory.GetCurrentDirectory();
            string indexPath = Path.Combine(functionDirectory, "angular-basic", "index.html");

            // Check if the file exists
            if (!File.Exists(indexPath))
            {
                _logger.LogError($"File not found: {indexPath}");
                return new NotFoundObjectResult($"File not found: {indexPath}");
            }

            // Read the contents of the index.html
            string htmlContent;
            using (StreamReader reader = new StreamReader(indexPath))
            {
                htmlContent = await reader.ReadToEndAsync();
            }

            // Generate a nonce (random value)
            string nonce = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 16);

            // Inject the nonce into <script> tags or other necessary places
            // Example: add 'nonce' to all <script> tags
            string updatedHtmlContent = htmlContent.Replace("<script", $"<script nonce=\"{nonce}\"");

            // You can also update CSP headers, inline styles, or other elements if needed

            // Optionally, write the updated file back to the original location
            // using (StreamWriter writer = new StreamWriter(indexPath, false, Encoding.UTF8))
            // {
            //     await writer.WriteAsync(updatedHtmlContent);
            // }

            // Return the updated content (or simply save it as shown above)
            return new OkObjectResult(updatedHtmlContent);
        }
    }
}
