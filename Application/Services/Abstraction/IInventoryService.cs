using Application.Dtos;
using Core.Entities;

namespace Application.Services.Abstraction
{
    public interface IInventoryService
    {
        Task<Inventory> AddInventoryAsync(InventoryDto inventoryDto);
        Task<IEnumerable<Inventory>> GetAllInventoryAsync();
        Task<Inventory> GetInventoryAsync(string id);
        Task<IEnumerable<Inventory>> GetLowStockItemsAsync(int threshold = 3);
        Task<bool> TakeInventoryItemAsync(string id, int quantityToTake);
        Task<(IEnumerable<Inventory> Items, string ContinuationToken)> LoadMoreInventoryAsync(string continuationToken, int? pageSize = null);
    }
}