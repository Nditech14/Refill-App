using Core.Enum;

namespace Core.Entities
{
    public class ItemRequestFor
    {
        public string id { get; set; } = Guid.NewGuid().ToString();
        public List<RequestItem> Items { get; set; } = new List<RequestItem>();
        public RequestStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
