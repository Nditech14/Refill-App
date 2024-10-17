namespace Application.Dtos
{
    public class ItemDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        public decimal TotalCost
        {
            get
            {
                return UnitPrice * Quantity;
            }
        }

    }
}
