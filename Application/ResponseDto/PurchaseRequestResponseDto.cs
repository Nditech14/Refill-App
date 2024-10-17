using Application.Dtos;
using Core.Entities;
using Core.Enum;

namespace Application.ResponseDto
{
    public class PurchaseRequestResponseDto
    {
        public string id { get; set; } = Guid.NewGuid().ToString();
        public List<Item> Items { get; set; } = new List<Item>();

        public string UserId { get; set; }
        public decimal TotalPrice => Items.Sum(item => item.UnitPrice * item.Quantity);
        public List<FileEntityDto> ReceiptImageUrl { get; set; } = new List<FileEntityDto>();
        public RequestStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
