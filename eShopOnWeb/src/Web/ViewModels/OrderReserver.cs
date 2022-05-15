using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.Web.ViewModels;

public class OrderReserver
{
    public string FileName { get; set; }
    public IEnumerable<OrderItemReserver> OrderItems { get; set; }
}

public class OrderItemReserver
{
    public string ItemId { get; set; }
    public int Quantity { get; set; }
}
