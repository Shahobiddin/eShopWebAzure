using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Blob.Infrastructure;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace OrderItemReserveServiceBusFunc
{
    public static class ServiceBusEndPoint
    {
        [FunctionName("StoreOrderItemsIntoBlob")]
        public static async Task Run([ServiceBusTrigger("ordermessages", Connection = "ServiceBusConnection")] string myQueueItem, ILogger log)
        {
            log.LogTrace($"C# ServiceBus queue trigger function processed message: {myQueueItem}");

            try
            {
                var throwException = false;
                bool.TryParse(Environment.GetEnvironmentVariable("ThrowException", EnvironmentVariableTarget.Process), out throwException);
                if (throwException)
                    throw new Exception("We want to check sending an email through Logic Apps...))");

                Order order = JsonConvert.DeserializeObject<Order>(myQueueItem);

                string jsonString = JsonConvert.SerializeObject(order.OrderItems);
                Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));

                log.LogInformation($"ServiceBusEndPoint Received a request: {JsonConvert.SerializeObject(order)}");

                AzureStorageConfig azureStorageConfig = new AzureStorageConfig()
                {
                    ConnectionString = Environment.GetEnvironmentVariable("BlobStorageConnectionString", EnvironmentVariableTarget.Process),
                    FileContainerName = Environment.GetEnvironmentVariable("BlobStorageContainerName", EnvironmentVariableTarget.Process)
                };

                BlobStorageService blobStorageService = new BlobStorageService(azureStorageConfig);
                await blobStorageService.Initialize();

                await blobStorageService.Save(stream, order.FileName);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                string? emailLogicAppEntryPoint = Environment.GetEnvironmentVariable("EmailLogicAppEntryPoint", EnvironmentVariableTarget.Process);
                await PostMessageAsync(emailLogicAppEntryPoint, myQueueItem);
            }
        }

        private static async Task PostMessageAsync(string? url, string body)
        {
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(url, content);
            }
        }
    }
}
