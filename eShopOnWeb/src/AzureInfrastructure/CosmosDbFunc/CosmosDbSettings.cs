using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryOrderProcessorCosmosDbFunc
{
    internal class CosmosDbSettings
    {
        public string Key { get; set; }
        public string EndpointUrl { get; set; }
        public string Database { get; set; }
        public string Collection { get; set; }
    }
}
