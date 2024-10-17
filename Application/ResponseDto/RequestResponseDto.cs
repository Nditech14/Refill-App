using Core.Entities;
using Core.Enum;

namespace Application.ResponseDto
{
    public class RequestResponseDto
    {
        public string id { get; set; } = Guid.NewGuid().ToString();
        public List<RequestItem> Items { get; set; } = new List<RequestItem>();
        public RequestStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
