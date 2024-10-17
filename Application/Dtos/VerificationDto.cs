namespace Application.Dtos
{
    public class VerificationDto
    {
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ReceiptImageUrl { get; set; }
    }
}
