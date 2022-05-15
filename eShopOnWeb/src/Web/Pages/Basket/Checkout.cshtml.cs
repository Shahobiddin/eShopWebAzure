using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Exceptions;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.Web.Features.OrderDetails;
using Microsoft.eShopWeb.Web.Interfaces;
using Microsoft.eShopWeb.Web.ViewModels;

namespace Microsoft.eShopWeb.Web.Pages.Basket;

[Authorize]
public class CheckoutModel : PageModel
{
    private string _username = null;
    private readonly IMediator _mediator;
    private readonly IOrderService _orderService;
    private readonly IBasketService _basketService;
    private readonly IStorageViewModelService _storageViewModelService;
    private readonly IBasketViewModelService _basketViewModelService;
    private readonly ICosmosDbViewModelService _cosmosDbViewModelService;
    private readonly IServiceBusViewModelService _serviceBusViewModelService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IAppLogger<CheckoutModel> _logger;

    public CheckoutModel(IBasketService basketService,
        IBasketViewModelService basketViewModelService,
        SignInManager<ApplicationUser> signInManager,
        IOrderService orderService,
        IStorageViewModelService storageViewModelService,
        IAppLogger<CheckoutModel> logger, IMediator mediator,
        ICosmosDbViewModelService cosmosDbViewModelService, IServiceBusViewModelService serviceBusViewModelService)
    {
        _basketService = basketService;
        _signInManager = signInManager;
        _orderService = orderService;
        _storageViewModelService = storageViewModelService;
        _basketViewModelService = basketViewModelService;
        _logger = logger;
        _mediator = mediator;
        _cosmosDbViewModelService = cosmosDbViewModelService;
        _serviceBusViewModelService = serviceBusViewModelService;
    }

    public BasketViewModel BasketModel { get; set; } = new BasketViewModel();

    public async Task OnGet()
    {
        await SetBasketModelAsync();
    }

    public async Task<IActionResult> OnPost(IEnumerable<BasketItemViewModel> items)
    {
        try
        {
            await SetBasketModelAsync();

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var updateModel = items.ToDictionary(b => b.Id.ToString(), b => b.Quantity);
            await _basketService.SetQuantities(BasketModel.Id, updateModel);
            int orderId = await _orderService.CreateOrderAsync(BasketModel.Id, new Address("123 Main St.", "Kent", "OH", "United States", "44240"));
            await _basketService.DeleteBasketAsync(BasketModel.Id);

            //Write to Azure Service Bus Queue
            IEnumerable<OrderItemReserver> orderItems = updateModel.Select(b => new OrderItemReserver() { ItemId = b.Key, Quantity = b.Value });
            OrderReserver order = new OrderReserver()
            {
                FileName = $"{BasketModel.Id}__{DateTime.UtcNow.ToString("dd-MM-yyy hh:mm:ss")}.json",
                OrderItems = orderItems
            };
            await _serviceBusViewModelService.QueueMessage(order);

            //Write to Azure CosmosDb
            _logger.LogInformation($"eShopWeb App created a new order: {orderId}");
            var viewModel = await _mediator.Send(new GetOrderDetails(User.Identity.Name, orderId));
            await _cosmosDbViewModelService.Process(viewModel);
        }
        catch (EmptyBasketOnCheckoutException emptyBasketOnCheckoutException)
        {
            //Redirect to Empty Basket page
            _logger.LogWarning(emptyBasketOnCheckoutException.Message);
            return RedirectToPage("/Basket/Index");
        }

        return RedirectToPage("Success");
    }

    private async Task SetBasketModelAsync()
    {
        if (_signInManager.IsSignedIn(HttpContext.User))
        {
            BasketModel = await _basketViewModelService.GetOrCreateBasketForUser(User.Identity.Name);
        }
        else
        {
            GetOrSetBasketCookieAndUserName();
            BasketModel = await _basketViewModelService.GetOrCreateBasketForUser(_username);
        }
    }

    private void GetOrSetBasketCookieAndUserName()
    {
        if (Request.Cookies.ContainsKey(Constants.BASKET_COOKIENAME))
        {
            _username = Request.Cookies[Constants.BASKET_COOKIENAME];
        }
        if (_username != null) return;

        _username = Guid.NewGuid().ToString();
        var cookieOptions = new CookieOptions();
        cookieOptions.Expires = DateTime.Today.AddYears(10);
        Response.Cookies.Append(Constants.BASKET_COOKIENAME, _username, cookieOptions);
    }
}
