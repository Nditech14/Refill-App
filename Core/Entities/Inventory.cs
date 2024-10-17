namespace Core.Entities
{
    public class Inventory
    {
        public string id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }

        public DateTime LastStocked { get; set; }
    }
}
