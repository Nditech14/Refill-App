using Application.Dtos;
using Application.ResponseDto;
using Core.Entities;
using Core.Enum;
using Microsoft.AspNetCore.Http;

namespace Application.Services.Abstraction
{
    public interface IPurchaseRequestService
    {
        Task<PurchaseRequest> CreatePurchaseRequestAsync(PurchaseRequestDto purchaseRequestDto);
        Task<bool> DeletePurchaseRequestAsync(string id);
        Task<PurchaseRequestResponseDto> EditPurchaseRequestAsync(string id, List<IFormFile> receiptImages);
        Task<IEnumerable<PurchaseRequestResponseDto>> GetAllPurchaseRequestsAsync();
        Task<(IEnumerable<PurchaseRequestResponseDto> Items, string ContinuationToken)> GetPaginatedPurchaseRequestsAsync(string continuationToken, int pageSize = 10);
        Task<(IEnumerable<PurchaseRequestResponseDto> Items, string ContinuationToken)> GetPaginatedPurchaseRequestsByStatusAsync(RequestStatus status, string continuationToken, int pageSize = 10);
        Task<PurchaseRequestResponseDto> GetPurchaseRequestAsync(string id);
        Task<IEnumerable<PurchaseRequestResponseDto>> GetPurchaseRequestsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<PurchaseRequest>> GetPurchaseRequestsByStatusAsync(RequestStatus status);
        Task<bool> UpdatePurchaseRequestStatusAsync(string id, RequestStatus status);
        Task<(IEnumerable<PurchaseRequest> Items, string ContinuationToken)> LoadMorePurchaseRequestsByStatusAsync(RequestStatus status, string continuationToken, int pageSize = 10);

        Task<PurchaseRequestResponseDto> PurChasedItems(string id, UpdatePurchasedItemsDto purchaseRequestDto);
    }
}