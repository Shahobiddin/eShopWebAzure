using Microsoft.eShopWeb.Web.Interfaces;
using Microsoft.eShopWeb.Web.ViewModels;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;

namespace Microsoft.eShopWeb.Web.Services;

public class ServiceBusViewModelService : IServiceBusViewModelService
{
    private readonly string _queueName;
    private readonly string _serviceBusConnection;
    private readonly ILogger<ServiceBusViewModelService> _logger;

    public ServiceBusViewModelService(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<ServiceBusViewModelService>();
        _queueName = configuration.GetValue<string>("ServiceBusQueueName") ?? throw new Exception("Service Bus Queue Name not provided.");
        _serviceBusConnection = configuration.GetConnectionString("ServiceBusConnection") ?? throw new Exception("Service Bus connection not provied.");
    }

    public async Task QueueMessage(OrderReserver order)
    {
        await using var client = new ServiceBusClient(_serviceBusConnection);

        await using ServiceBusSender sender = client.CreateSender(_queueName);
        try
        {
            string messageBody = JsonConvert.SerializeObject(order);
            var message = new ServiceBusMessage(messageBody);
            _logger.LogInformation($"eShopWeb Service Bus Message: {messageBody}");
            await sender.SendMessageAsync(message);
        }
        catch (Exception exception)
        {
            _logger.LogError($"eShopWeb Service Bus Exception: {exception.Message}, {DateTime.Now}");
        }
        finally
        {
            await sender.DisposeAsync();
            await client.DisposeAsync();
        }
    }
}
