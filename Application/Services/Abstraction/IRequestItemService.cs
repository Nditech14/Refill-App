using Core.Entities;
using Core.Enum;

namespace Application.Services.Abstraction
{
    public interface IRequestItemService
    {
        Task<ItemRequestFor> CreateRequestAsync(ItemRequestFor request);
        Task<bool> DeleteRequestAsync(string id);
        Task<IEnumerable<ItemRequestFor>> GetAllRequestsAsync();
        Task<ItemRequestFor> GetRequestAsync(string id);
        Task<bool> UpdateRequestStatusAsync(string requestId, RequestStatus newStatus);
    }
}