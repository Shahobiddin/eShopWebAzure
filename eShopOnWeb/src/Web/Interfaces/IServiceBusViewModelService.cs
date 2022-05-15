using Microsoft.eShopWeb.Web.ViewModels;

namespace Microsoft.eShopWeb.Web.Interfaces;

public interface IServiceBusViewModelService
{
    public Task QueueMessage(OrderReserver order);
}
