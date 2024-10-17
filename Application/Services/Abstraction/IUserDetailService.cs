using Application.Dtos;
using Core.Entities;

namespace Application.Services.Abstraction
{
    public interface IUserDetailService
    {
        Task AddUserAsync(UserDetailsDto userDetailsDto);
        Task DeleteUserAsync(string id);
        Task<IEnumerable<string>> GetAdminEmailsAsync();
        Task<IEnumerable<UserDetails>> GetAllUsersAsync();
        Task<UserDetails> GetUserByIdAsync(string id);
        Task<UserDetails> GetUserByUserIdAsync(string userId);
        Task<IEnumerable<UserDetails>> GetUsersByRoleAsync(string role);
        Task UpdateUserAsync(string id, UserDetailsDto userDetailsDto);
        Task<IEnumerable<string>> GetUserEmailsAsync();
        Task<string> GetUserEmailByIdAsync(string UserId);
        Task<string> GetUserLastNameByIdAsync(string UserId);
        Task<string> GetUserFirstNameByIdAsync(string UserId);
        Task<string> GetAdminByIdAsync(string UserId);
    }
}