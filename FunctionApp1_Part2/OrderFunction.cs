using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using System;
using System.Net;
using System.Threading.Tasks;
using Azure.Storage.Queues;

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
    public static class OrderFunctions
    {
        private const string ConnectionName = "AzureWebJobsStorage";
        private const string OrderTableName = "orders";
        private const string EventsQueueName = "order-events";
        private const string LogQueueName = "order-events-log";

        // POST: api/orders/create
        // Function for azure table "orders" and Queues "order-events" and "order-events-log"
        [FunctionName("ProcessNewOrder")]
        public static async Task<IActionResult> ProcessNewOrder(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "orders/create")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function 'ProcessNewOrder' received a request.");

            try
            {
                string orderId = Guid.NewGuid().ToString();
                string message = $"New Order Created: {orderId}";

                // 1. Insert into "orders" Table (Direct Service Call via Azure SDK)
                var tableClient = new TableClient(Environment.GetEnvironmentVariable(ConnectionName), OrderTableName);
                await tableClient.CreateIfNotExistsAsync();

                var newOrder = new TableEntity("Order", orderId)
                {
                    { "Status", "Pending" },
                    { "OrderDate", DateTimeOffset.UtcNow }
                };
                await tableClient.AddEntityAsync(newOrder);

                // 2. Enqueue message to "order-events" (for processing)
                var eventsQueueClient = new QueueClient(Environment.GetEnvironmentVariable(ConnectionName), EventsQueueName);
                await eventsQueueClient.CreateIfNotExistsAsync();
                await eventsQueueClient.SendMessageAsync(message);

                // 3. Enqueue message to "order-events-log" (for auditing/logging)
                var logQueueClient = new QueueClient(Environment.GetEnvironmentVariable(ConnectionName), LogQueueName);
                await logQueueClient.CreateIfNotExistsAsync();
                await logQueueClient.SendMessageAsync(message);

                return new OkObjectResult($"Order {orderId} processed. Table entry created and messages enqueued to {EventsQueueName} and {LogQueueName}.");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error processing new order: " + ex.Message);
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}