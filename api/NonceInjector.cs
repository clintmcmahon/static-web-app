using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.RegularExpressions;
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

            // Replace the CSP_NONCE placeholder in index.html with the generated nonce
            string updatedHtmlContent = htmlContent.Replace("{{CSP_NONCE}}", nonce);

            // Inject nonce into all <script> tags using a regex
            string scriptPattern = @"<script\s*";
            updatedHtmlContent = Regex.Replace(updatedHtmlContent, scriptPattern, $"<script nonce=\"{nonce}\" ", RegexOptions.IgnoreCase);

            // Set the CSP header to allow inline scripts with the generated nonce
            var headers = req.HttpContext.Response.Headers;
            headers.Add("Content-Security-Policy", $"default-src 'self'; script-src 'nonce-{nonce}' 'self'; style-src 'nonce-{nonce}' 'self';");

            // Return the updated HTML content
            return new ContentResult
            {
                Content = updatedHtmlContent,
                ContentType = "text/html",
                StatusCode = StatusCodes.Status200OK
            };
        }
    }
}
