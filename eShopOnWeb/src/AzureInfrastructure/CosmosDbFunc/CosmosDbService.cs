using DeliveryOrderProcessorCosmosDbFunc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryOrderProcessorCosmosDbFunc
{


    internal class CosmosDbService
    {
        private readonly CosmosDbSettings _dbSettings;
        private DocumentClient client;


        public CosmosDbService(CosmosDbSettings dbSettings)
        {
            this._dbSettings = dbSettings;
        }

        public async Task Initialize()
        {
            client = new DocumentClient(new Uri(_dbSettings.EndpointUrl), _dbSettings.Key);

            await client.CreateDatabaseIfNotExistsAsync(new Database { Id = _dbSettings.Database });

            await client.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(_dbSettings.Database),
                new DocumentCollection { Id = _dbSettings.Collection }
            );

            Console.WriteLine("Database and collection validation complete");
        }

        public async Task SaveToDb(OrderViewModel order)
        {
            await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(_dbSettings.Database, _dbSettings.Collection), order);
        }
    }
}
