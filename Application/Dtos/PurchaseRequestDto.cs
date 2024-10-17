using Core.Enum;

namespace Application.Dtos
{
    public class PurchaseRequestDto
    {
        public List<ItemDto> Items { get; set; } = new List<ItemDto>();



        public RequestStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
