//using Core.Abstraction;
//using Core.Entities;

//namespace Re_Fill_Web_API.Seeding
//{
//    public class AdminSeeder
//    {
//        private readonly ICosmosDbRepository<UserDetails> _userDetailsRepository;

//        public AdminSeeder(ICosmosDbRepository<UserDetails> userDetailsRepository)
//        {
//            _userDetailsRepository = userDetailsRepository;
//        }

//        public async Task SeedAdminUserAsync()
//        {
//            // Check if the admin already exists
//            var adminUserId = "8ba2dbd6-f309-444b-b7ec-327bff20277b";  // Replace with actual admin's UserId
//            var existingAdmin = await _userDetailsRepository.GetItemAsyncz(adminUserId);

//            if (existingAdmin == null)
//            {
//                // Create hard-coded admin user
//                var adminDetails = new UserDetails
//                {
//                    userId = adminUserId,
//                    FirstName = "Precious",
//                    LastName = "Dominic",
//                    Email = "pdominic@Infinion.co",
//                    Role = "Admin"
//                };

//                // Save the admin user to Cosmos DB
//                await _userDetailsRepository.AddItemAsync(adminDetails);
//            }
//        }
//    }
//}
