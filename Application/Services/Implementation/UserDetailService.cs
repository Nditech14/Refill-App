using Application.Dtos;
using Application.Services.Abstraction;
using AutoMapper;
using Core.Entities;

namespace Application.Services
{
    public class UserDetailService : IUserDetailService
    {
        private readonly ICosmosDbService<UserDetails> _cosmosDbService;
        private readonly IMapper _mapper;

        public UserDetailService(ICosmosDbService<UserDetails> cosmosDbService, IMapper mapper)
        {
            _cosmosDbService = cosmosDbService;
            _mapper = mapper;
        }

        public async Task<UserDetails> GetUserByIdAsync(string id)
        {
            return await _cosmosDbService.GetItemAsync(id, id);
        }

        public async Task<IEnumerable<UserDetails>> GetAllUsersAsync()
        {
            return await _cosmosDbService.GetItemsAsync("SELECT * FROM c");
        }

        public async Task AddUserAsync(UserDetailsDto userDetailsDto)
        {
            var userDetails = _mapper.Map<UserDetails>(userDetailsDto);
            var existingUser = await GetUserByUserIdAsync(userDetails.UserId);
            if (existingUser != null)
            {
                throw new System.Exception($"User with UserId {userDetails.UserId} already exists.");
            }

            await _cosmosDbService.AddItemAsync(userDetails, userDetails.id);
        }

        public async Task UpdateUserAsync(string id, UserDetailsDto userDetailsDto)
        {
            var userDetails = _mapper.Map<UserDetails>(userDetailsDto);
            await _cosmosDbService.UpdateItemAsync(id, userDetails, id);
        }

        public async Task DeleteUserAsync(string id)
        {
            await _cosmosDbService.DeleteItemAsync(id, id);
        }

        public async Task<UserDetails> GetUserByUserIdAsync(string userId)
        {
            var query = $"SELECT * FROM c WHERE c.userId = '{userId}'";
            var users = await _cosmosDbService.GetItemsAsync(query);
            return users.FirstOrDefault();
        }


        public async Task<IEnumerable<UserDetails>> GetUsersByRoleAsync(string role)
        {
            var query = $"SELECT * FROM c WHERE c.Role = '{role}'";
            var users = await _cosmosDbService.GetItemsAsync(query);
            return users;
        }

        public async Task<IEnumerable<string>> GetAdminEmailsAsync()
        {
            var admins = await GetUsersByRoleAsync("Admin");
            return admins.Select(admin => admin.Email).ToList();
        }
        public async Task<IEnumerable<string>> GetUserEmailsAsync()
        {
            var users = await GetUsersByRoleAsync("User");
            return users.Select(user => user.Email).ToList();
        }
        public async Task<string> GetUserEmailByIdAsync(string UserId)
        {

            var user = await GetUserByUserIdAsync(UserId);


            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {UserId} not found.");
            }

            return user.Email;
        }
        public async Task<string> GetUserFirstNameByIdAsync(string UserId)
        {

            var user = await GetUserByUserIdAsync(UserId);


            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {UserId} not found.");
            }

            return user.FirstName;
        }
        public async Task<string> GetUserLastNameByIdAsync(string UserId)
        {

            var user = await GetUserByUserIdAsync(UserId);


            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {UserId} not found.");
            }

            return user.LastName;
        }
        public async Task<string> GetAdminByIdAsync(string UserId)
        {

            var user = await GetUserByUserIdAsync(UserId);


            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {UserId} not found.");
            }

            return user.Role;
        }
    }
}
