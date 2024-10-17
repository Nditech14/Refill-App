using Core.Enum;

namespace Core.Entities
{
    public class PurchaseRequest
    {
        public string id { get; set; } = Guid.NewGuid().ToString();
        public List<Item> Items { get; set; } = new List<Item>();

        public string UserId { get; set; }
        public decimal TotalPrice => Items.Sum(item => item.UnitPrice * item.Quantity);
        public List<FileEntity> ReceiptImageUrl { get; set; } = new List<FileEntity>();
        public RequestStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
