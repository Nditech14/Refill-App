using Newtonsoft.Json;

namespace Core.Entities
{
    public class UserDetails
    {
        public string id { get; set; } = Guid.NewGuid().ToString();
        [JsonProperty("userId")]
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
