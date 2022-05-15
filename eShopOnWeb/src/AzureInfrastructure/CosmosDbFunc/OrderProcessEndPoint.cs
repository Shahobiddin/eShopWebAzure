using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DeliveryOrderProcessorCosmosDbFunc
{
    public static class OrderProcessEndPoint
    {
        [FunctionName("StoreOrdersIntoCosmosDb")]
        public static async Task<IActionResult> Run([HttpTrigger("POST")] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("OrderStoreEndPoint HTTP trigger function processed a request.");

                CosmosDbSettings cosmosDbSettings = new CosmosDbSettings()
                {
                    Key = Environment.GetEnvironmentVariable("CosmosDbAccountKey", EnvironmentVariableTarget.Process),
                    EndpointUrl = Environment.GetEnvironmentVariable("CosmosDbEndpointUrl", EnvironmentVariableTarget.Process),
                    Database = Environment.GetEnvironmentVariable("CosmosDbName", EnvironmentVariableTarget.Process),
                    Collection = Environment.GetEnvironmentVariable("CosmosDbCollection", EnvironmentVariableTarget.Process)
                };

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                OrderViewModel order = JsonConvert.DeserializeObject<OrderViewModel>(requestBody);

                CosmosDbService cosmosDbService = new CosmosDbService(cosmosDbSettings);
                await cosmosDbService.Initialize();
                await cosmosDbService.SaveToDb(order);

                log.LogInformation($"OrderStoreEndPoint Order:  {order.ToString()}");

                return new OkObjectResult("Order saved successfully...");
            }
            catch (Exception ex)
            {
                log.LogError($"OrderStoreEndPoint Error: {ex.Message}");
                return new ObjectResult(ex.Message);
            }
        }
    }
}
