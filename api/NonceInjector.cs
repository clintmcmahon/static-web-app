using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Security.Cryptography;
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
            var nonce = GenerateNonce();
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            // Define the path to the index.html in the "angular-basic" folder
            string functionDirectory = Directory.GetCurrentDirectory();
            string filePath = Path.Combine(functionDirectory, "angular-basic", "index.html");


            string htmlContent;

            if (File.Exists(filePath))
            {
                htmlContent = await File.ReadAllTextAsync(filePath);
            }
            else
            {
                return new NotFoundObjectResult("Index file not found.");
            }

            // Replace nonce placeholder in the HTML file
            htmlContent = htmlContent.Replace("DYNAMIC_NONCE_VALUE", nonce);

            // Add the CSP header with the nonce value
            var result = new ContentResult
            {
                Content = htmlContent,
                ContentType = "text/html",
            };

            req.HttpContext.Response.Headers.Add("Content-Security-Policy", $"script-src 'self' 'nonce-{nonce}'");

            return result;
        }

        private static string GenerateNonce()
        {
            byte[] nonceBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(nonceBytes);
            }

            // Convert to Base64 for a URL-safe nonce
            return Convert.ToBase64String(nonceBytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }
}
