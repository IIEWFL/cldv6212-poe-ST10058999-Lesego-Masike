using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using System;
using System.Net;

// References
//Microsoft(2025a) Azure SQL Database geo-replication overview. Available at: https://learn.microsoft.com/en-us/azure/azure-sql/database/active-geo-replication-overview (Accessed: 9 November 2025).
//Microsoft(2025b) Read scale-out with Azure SQL Database. Available at: https://learn.microsoft.com/en-us/azure/azure-sql/database/read-scale-out (Accessed: 9 November 2025).

//Microsoft(2025c) Automated backups - Azure SQL Database. Available at: https://learn.microsoft.com/en-us/azure/azure-sql/database/automated-backups-overview (Accessed: 9 November 2025).

//Microsoft(2025d) What is Azure SQL Database?.Available at: https://learn.microsoft.com/en-us/azure/azure-sql/database/sql-database-paas-overview (Accessed: 9 November 2025).

//Microsoft(2025e) Azure Functions consumption plan hosting. Available at: https://learn.microsoft.com/en-us/azure/azure-functions/consumption-plan (Accessed: 9 November 2025).

//Microsoft(2025f) Azure Blob Storage introduction. Available at: https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blobs-introduction (Accessed: 9 November 2025).

//Microsoft(2025g) What is Azure Active Directory B2C?. Available at: https://learn.microsoft.com/en-us/azure/active-directory-b2c/overview (Accessed: 9 November 2025).

//Microsoft(2025h) What is Application Insights ?.Available at: https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview (Accessed: 9 November 2025).

//Microsoft(2025i) Deploy to Azure from GitHub Actions. Available at: https://learn.microsoft.com/en-us/azure/developer/github/connect-to-azure (Accessed: 9 November 2025).

//Microsoft(2025j) Azure Cosmos DB SQL API. Available at: https://learn.microsoft.com/en-us/azure/cosmos-db/sql-api-introduction (Accessed: 9 November 2025).

//Microsoft(2025k) What is Azure Logic Apps?.Available at: https://learn.microsoft.com/en-us/azure/logic-apps/logic-apps-overview (Accessed: 9 November 2025).

//Microsoft(2025l) Introduction to Azure Data Lake Storage Gen2. Available at: https://learn.microsoft.com/en-us/azure/storage/blobs/data-lake-storage-introduction (Accessed: 9 November 2025).

//Microsoft(2025m) What is Azure Active Directory?.Available at: https://learn.microsoft.com/en-us/azure/active-directory/fundamentals/active-directory-whatis (Accessed: 9 November 2025).

//Microsoft(2025n) Azure Monitor overview. Available at: https://learn.microsoft.com/en-us/azure/azure-monitor/overview (Accessed: 9 November 2025).

//Microsoft(2025o) What is Azure Pipelines ?.Available at: https://learn.microsoft.com/en-us/azure/devops/pipelines/get-started/what-is-azure-pipelines (Accessed: 9 November 2025).

//Microsoft(2025p) IHttpClientFactory with ASP.NET Core. Available at: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests (Accessed: 9 November 2025).

//Microsoft(2025q) System.Text.Json namespace.Available at: https://learn.microsoft.com/en-us/dotnet/api/system.text.json (Accessed: 9 November 2025).

//Microsoft(2025r) Develop Azure Functions using .NET isolated worker process.Available at: https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide (Accessed: 9 November 2025).

//Microsoft(2025s) Entity Framework Core documentation.Available at: https://learn.microsoft.com/en-us/ef/core/ (Accessed: 9 November 2025).

namespace FunctionApp1_Part2
{
    public static class ProductFunctions
    {
        private const string ConnectionName = "AzureWebJobsStorage";
        private const string ImageContainerName = "product-images";
        // The 'products' table interaction can also be moved here if CRUD operations are centralized.

        // POST: api/products/upload-image
        // Function for Azure Table "products" and Blob Storage "product-images"
        [FunctionName("UploadProductImage")]
        public static async Task<IActionResult> UploadProductImage(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "products/upload-image")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function 'UploadProductImage' received a request.");

            try
            {
                // 1. Get the file from the request
                if (req.Form.Files.Count == 0)
                {
                    return new BadRequestObjectResult("Please include an image file in the request.");
                }

                var file = req.Form.Files[0];
                // Use a unique name to prevent collisions if the client is not careful
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                var blobClient = new BlobContainerClient(
                    Environment.GetEnvironmentVariable(ConnectionName),
                    ImageContainerName
                );

                await blobClient.CreateIfNotExistsAsync();

                var blob = blobClient.GetBlobClient(fileName);

                using (var stream = file.OpenReadStream())
                {
                    await blob.UploadAsync(stream, overwrite: true);
                }

                var uri = blob.Uri.ToString();
                log.LogInformation($"Successfully uploaded image to: {uri}");

                // Return the public URL of the uploaded blob
                return new OkObjectResult(new { imageUrl = uri });
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error uploading product image: " + ex.Message);
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}