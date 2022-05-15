using System.Net;
using Microsoft.eShopWeb.Web.Interfaces;
using Microsoft.eShopWeb.Web.ViewModels;

namespace Microsoft.eShopWeb.Web.Services;

public class StorageViewModelService : IStorageViewModelService
{
    private readonly string _azureBaseFuncUrl = "https://cosmosdbfunc1739.azurewebsites.net";
    private readonly string _funcUrl = "/api/OrderStoreEndPoint?code=qoCR2L2drJZQZS5wCK3B1qYuR5wrgAw6ojlu1lAGhtlJUIYkNCputg==";
    public async Task SaveToBlob(OrderReserver order)
    {
        using (HttpClient httpClient = new HttpClient())
        {
            httpClient.BaseAddress = new Uri(_azureBaseFuncUrl);
            await httpClient.PostAsJsonAsync<OrderReserver>(_funcUrl, order);
        }
    }
}
