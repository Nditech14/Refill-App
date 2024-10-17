using Core.Abstraction;
using Core.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace Core.Implementation
{

    public class CosmosDbRepository<T> : ICosmosDbRepository<T>
    {
        private readonly Container _InventoryContainer;
        private readonly Container _PurchaseRequestContainer;
        private readonly Container _UserDetailsContainer;
        private readonly Container _RequestedItemContainer;


        public CosmosDbRepository(CosmosClient dbClient, IConfiguration configuration)
        {
            var databaseName = configuration["CosmosDb:DatabaseName"];
            _InventoryContainer = dbClient.GetContainer(databaseName, configuration["CosmosDb:Containers:InventoryContainer"]);
            _PurchaseRequestContainer = dbClient.GetContainer(databaseName, configuration["CosmosDb:Containers:PurchaseRequestContainer"]);
            _UserDetailsContainer = dbClient.GetContainer(databaseName, configuration["CosmosDb:Containers:UserDetailsContainer"]);
            _RequestedItemContainer = dbClient.GetContainer(databaseName, configuration["CosmosDb:Containers:RequestedItemContainer"]);

        }


        private Container GetContainer()
        {
            if (typeof(T) == typeof(Inventory))
            {
                return _InventoryContainer;
            }
            else if (typeof(T) == typeof(UserDetails))
            {
                return _UserDetailsContainer;
            }
            else if (typeof(T) == typeof(ItemRequestFor))
            {
                return _RequestedItemContainer;
            }
            else if (typeof(T) == typeof(PurchaseRequest))
            {
                return _PurchaseRequestContainer;
            }

            else
                throw new ArgumentException($"No container available for type {typeof(T).Name}");
            {
            }
        }


        public async Task<T> GetItemAsync(string id, PartitionKey partitionKey)
        {
            var container = GetContainer();
            try
            {
                var response = await container.ReadItemAsync<T>(id, partitionKey);
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return default;
            }
        }


        public async Task<IEnumerable<T>> GetItemsAsync(string queryString)
        {
            var _container = GetContainer();
            var query = _container.GetItemQueryIterator<T>(new QueryDefinition(queryString));
            List<T> results = new List<T>();

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response.ToList());
            }
            return results;
        }


        public async Task AddItemAsync(T item)
        {
            var _container = GetContainer();
            await _container.CreateItemAsync(item);
        }





        public async Task AddItemAsync(T item, string partitionKey)
        {
            var _container = GetContainer();
            await _container.CreateItemAsync(item, new PartitionKey(partitionKey));
        }


        public async Task UpdateItemAsync(string id, T item, string partitionKey)
        {
            var _container = GetContainer();
            await _container.UpsertItemAsync(item, new PartitionKey(partitionKey));
        }


        public async Task DeleteItemAsync(string id, string partitionKey)
        {
            var _container = GetContainer();
            await _container.DeleteItemAsync<T>(id, new PartitionKey(partitionKey));
        }


        public async Task<(IEnumerable<T> Items, string ContinuationToken)> GetItemsWithContinuationTokenAsync(string continuationToken, int maxItemCount = 30, string partitionKey = null)
        {
            var _container = GetContainer();
            var queryRequestOptions = new QueryRequestOptions { MaxItemCount = maxItemCount };

            if (partitionKey != null)
            {
                queryRequestOptions.PartitionKey = new PartitionKey(partitionKey);
            }

            var queryIterator = _container.GetItemQueryIterator<T>(continuationToken: continuationToken, requestOptions: queryRequestOptions);
            var results = new List<T>();
            string newContinuationToken = null;

            while (queryIterator.HasMoreResults)
            {
                var response = await queryIterator.ReadNextAsync();
                results.AddRange(response);
                newContinuationToken = response.ContinuationToken;

                if (results.Count >= maxItemCount)
                {
                    break;
                }
            }

            return (results, newContinuationToken);
        }


        public FeedIterator<T> GetItemQueryIterator(QueryDefinition query, string continuationToken = null, QueryRequestOptions requestOptions = null)
        {
            var _container = GetContainer();
            return _container.GetItemQueryIterator<T>(query, continuationToken, requestOptions);
        }

        public async Task<IEnumerable<T>> SearchItemsByNameAsync(string name)
        {
            var _container = GetContainer();


            var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.Name = @name")
                                    .WithParameter("@name", name);

            var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);
            var results = new List<T>();


            while (queryIterator.HasMoreResults)
            {
                var response = await queryIterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task<T> SearchItemByNameAsync(string name)
        {
            var container = GetContainer();
            var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.Name = @name")
                                    .WithParameter("@name", name);

            var queryIterator = container.GetItemQueryIterator<T>(queryDefinition);

            if (queryIterator.HasMoreResults)
            {
                var response = await queryIterator.ReadNextAsync();
                return response.FirstOrDefault();
            }

            return default;
        }

        public async Task<IEnumerable<T>> GetItemsWithLowStockAsync(int threshold)
        {
            var _container = GetContainer();


            var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.Quantity < @threshold")
                                    .WithParameter("@threshold", threshold);

            var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);
            var results = new List<T>();


            while (queryIterator.HasMoreResults)
            {
                var response = await queryIterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return results;
        }
        public async Task<(IEnumerable<T> Items, string ContinuationToken)> GetItemsWithContinuationTokenAsyncz(string continuationToken, int maxItemCount = 30, string query = null, string partitionKey = null)
        {
            var _container = GetContainer();
            var queryRequestOptions = new QueryRequestOptions { MaxItemCount = maxItemCount };

            if (partitionKey != null)
            {
                queryRequestOptions.PartitionKey = new PartitionKey(partitionKey);
            }

            // Query based on the provided query string (e.g., filtering by status)
            var queryIterator = _container.GetItemQueryIterator<T>(new QueryDefinition(query), continuationToken: continuationToken, requestOptions: queryRequestOptions);

            var results = new List<T>();
            string newContinuationToken = null;

            while (queryIterator.HasMoreResults)
            {
                var response = await queryIterator.ReadNextAsync();
                results.AddRange(response);
                newContinuationToken = response.ContinuationToken;

                if (results.Count >= maxItemCount)
                {
                    break;
                }
            }

            return (results, newContinuationToken);
        }




    }
}
