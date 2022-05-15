using Microsoft.eShopWeb.Web.Interfaces;
using Microsoft.eShopWeb.Web.ViewModels;

namespace Microsoft.eShopWeb.Web.Services;

public class CosmosDbViewModelService : ICosmosDbViewModelService
{
    private readonly string _funcUrl;
    private readonly string _funcBaseUrl;

    public CosmosDbViewModelService(IConfiguration configuration)
    {
        _funcUrl = configuration.GetConnectionString("DeliveryOrderProcessorFuncUrl") ?? throw new Exception("DeliveryOrderProcessorFuncUrl value not provided in appsettings");
        _funcBaseUrl = configuration.GetConnectionString("DeliveryOrderProcessorFuncBaseUrl") ?? throw new Exception("DeliveryOrderProcessorFuncBaseUrl value not provided in appsettings");
    }

    public async Task Process(OrderViewModel order)
    {
        using (HttpClient httpClient = new HttpClient())
        {
            httpClient.BaseAddress = new Uri(_funcBaseUrl);
            await httpClient.PostAsJsonAsync<OrderViewModel>(_funcUrl, order);
        }
    }
}
