using Application.Dtos;
using Application.Services.Abstraction;
using AutoMapper;
using Core.Entities;
using Microsoft.AspNetCore.Http;

namespace Application.Services.Implementation
{
    public class InventoryService : IInventoryService
    {
        private readonly ICosmosDbService<Inventory> _cosmosDbService;
        private readonly IMapper _mapper;
        private const int DefaultPageSize = 10;
        private readonly IServiceBusProducer _serviceBusProducer;
        private readonly IEmailService _emailService;
        private readonly IUserDetailService _userDetailService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public InventoryService(ICosmosDbService<Inventory> cosmosDbService, IMapper mapper, IServiceBusProducer serviceBusProducer, IUserDetailService userDetailService, IHttpContextAccessor httpContextAccessor, IEmailService emailService)
        {
            _cosmosDbService = cosmosDbService;
            _mapper = mapper;
            _serviceBusProducer = serviceBusProducer;
            _userDetailService = userDetailService;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
        }

        public async Task<IEnumerable<Inventory>> GetAllInventoryAsync()
        {
            var query = "SELECT * FROM c";
            var inventories = await _cosmosDbService.GetItemsAsync(query);
            return inventories.Select(i => new Inventory
            {
                id = i.id,
                Name = i.Name,
                Quantity = i.Quantity,
                Description = i.Description
            });
        }

        public async Task<Inventory> GetInventoryAsync(string id)
        {
            var inventory = await _cosmosDbService.GetItemAsync(id, id);
            if (inventory == null) return null;

            return new Inventory
            {
                id = id,
                Name = inventory.Name,
                Quantity = inventory.Quantity,
                Description = inventory.Description
            };
        }

        public async Task<Inventory> AddInventoryAsync(InventoryDto inventoryDto)
        {

            var inventory = _mapper.Map<Inventory>(inventoryDto);


            var existingInventory = await _cosmosDbService.SearchItemByNameAsync(inventory.Name);

            if (existingInventory != null)
            {

                existingInventory.Quantity += inventory.Quantity;
                existingInventory.LastStocked = DateTime.UtcNow;

                await _cosmosDbService.UpdateItemAsync(existingInventory.id, existingInventory, existingInventory.id);


                return existingInventory;
            }
            else
            {

                var newInventory = new Inventory
                {
                    Name = inventoryDto.Name,
                    Quantity = inventoryDto.Quantity,
                    Description = inventoryDto.Description

                };


                await _cosmosDbService.AddItemAsync(newInventory);


                return newInventory;
            }
        }

        public async Task<IEnumerable<Inventory>> GetLowStockItemsAsync(int threshold = 3)
        {
            var lowStockItems = await _cosmosDbService.GetItemsWithLowStockAsync(threshold);
            return lowStockItems.Select(i => new Inventory
            {
                id = i.id,
                Name = i.Name,
                Quantity = i.Quantity,
                Description = i.Description
            });
        }

        public async Task<bool> TakeInventoryItemAsync(string id, int quantityToTake)
        {
            var email = _httpContextAccessor.HttpContext.User.FindFirst("preferred_username")?.Value;
            var fullName = _httpContextAccessor.HttpContext.User.FindFirst("name")?.Value;
            var existingItem = await _cosmosDbService.GetItemAsync(id, id);

            if (existingItem == null)
            {

                throw new Exception("Item not found.");
            }


            if (existingItem.Quantity < quantityToTake)
            {

                throw new Exception($"Insufficient stock. Only {existingItem.Quantity} items available.");
            }


            existingItem.Quantity -= quantityToTake;


            await _cosmosDbService.UpdateItemAsync(existingItem.id, existingItem, existingItem.id);

            return true;
        }

        public async Task<(IEnumerable<Inventory> Items, string ContinuationToken)> LoadMoreInventoryAsync(string continuationToken, int? pageSize = null)
        {


            int maxItemCount = pageSize ?? DefaultPageSize;

            var result = await _cosmosDbService.GetItemsWithContinuationTokenAsync(continuationToken, maxItemCount);


            return (result.Items.Select(i => new Inventory
            {
                id = i.id,
                Name = i.Name,
                Quantity = i.Quantity,
                Description = i.Description
            }), result.ContinuationToken);
        }


    }
}
