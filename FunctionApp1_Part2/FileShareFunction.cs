using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace  FunctionApp1_Part2

{
    public static class FileShareFunctions
    {
        // POST: api/fileshare/process-contract
        // Function for azure file share "contracts" (e.g., triggering processing after upload)
        [FunctionName("ProcessFileShareContract")]
        public static async Task<IActionResult> ProcessFileShareContract(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "fileshare/process-contract")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function 'ProcessFileShareContract' received a request.");

            try
            {
                // The request body would contain the fileName or path on the share
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                // Logic to perform background processing on the file in the 'contracts' share 
                // using the file name provided in the request body.

                log.LogInformation($"Successfully triggered processing for file details: {requestBody}");

                return new OkObjectResult($"Processing of contract triggered for: {requestBody}");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error processing file share contract: " + ex.Message);
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}