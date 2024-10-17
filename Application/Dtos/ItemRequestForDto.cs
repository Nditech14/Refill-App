using Core.Enum;

namespace Application.Dtos
{
    public class ItemRequestForDto
    {

        public List<RequestItemDto> Items { get; set; } = new List<RequestItemDto>();

        public RequestStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
