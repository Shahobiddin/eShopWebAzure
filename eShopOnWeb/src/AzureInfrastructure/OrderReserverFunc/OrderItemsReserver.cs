using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections;
using System.Text;
using Blob.Infrastructure;

namespace OrderReserverBlobStorageFunc
{
    public static class OrderItemsReserver
    {
        static readonly string _blobStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=orderrequeststorage1216;AccountKey=rv2ajSU44ePm2NSg0XMsTFb3utpd8aPrp5YBPvyedpWQiwdqjscslHStJNAaN1RUy3Nt+ZLtb2NK+AStgUEyzA==;EndpointSuffix=core.windows.net";
        static readonly string _blobContainerName = "order-requests";

        [FunctionName("OrderItemsReserver")]
        public static async Task<IActionResult> Run(
            [HttpTrigger("POST")] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("Received a request");

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                Order order = JsonConvert.DeserializeObject<Order>(requestBody);

                string jsonString = JsonConvert.SerializeObject(order.OrderItems);
                Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));

                AzureStorageConfig azureStorageConfig = new AzureStorageConfig() { ConnectionString = _blobStorageConnectionString, FileContainerName = _blobContainerName };

                BlobStorageService blobStorageService = new BlobStorageService(azureStorageConfig);
                await blobStorageService.Initialize();

                await blobStorageService.Save(stream, order.FileName);

                return new OkObjectResult("json file uploaded...");
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                return new ObjectResult(ex.Message);
            }
        }
    }
}
