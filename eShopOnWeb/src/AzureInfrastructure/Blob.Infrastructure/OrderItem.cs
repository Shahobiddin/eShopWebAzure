using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blob.Infrastructure
{
    public class Order
    {
        public string FileName { get; set; }
        public IEnumerable<OrderItem> OrderItems { get; set; }
    }

    public class OrderItem
    {
        public string ItemId { get; set; }
        public int Quantity { get; set; }
    }
}
