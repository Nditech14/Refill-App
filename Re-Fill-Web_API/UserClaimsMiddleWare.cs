using Application.Dtos;
using Application.Services.Abstraction;
using System.Security.Claims;

namespace Re_Fill_Web_API
{
    public class UserClaimsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _scopeFactory;

        public UserClaimsMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _scopeFactory = scopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var userDetailService = scope.ServiceProvider.GetRequiredService<IUserDetailService>();

                    if (context.User.Identity.IsAuthenticated)
                    {
                        await ProcessUserClaimsAsync(context, userDetailService);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UserClaimsMiddleware: {ex.Message}");
            }
            finally
            {
                await _next(context);
            }
        }

        private async Task ProcessUserClaimsAsync(HttpContext context, IUserDetailService userDetailService)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = context.User.FindFirst("preferred_username")?.Value;
            var fullName = context.User.FindFirst("name")?.Value;
            var surname = context.User.FindFirstValue(ClaimTypes.Surname);
            var rolesClaim = context.User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToList();


            Console.WriteLine($"UserId: {userId}, Email: {email}, FullName: {fullName}, Surname: {surname}, Roles: {string.Join(", ", rolesClaim)}");

            string firstName = string.Empty;
            string lastName = string.Empty;

            if (!string.IsNullOrEmpty(fullName))
            {
                var nameParts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                firstName = nameParts[0];
                if (nameParts.Length > 1)
                {
                    lastName = string.Join(" ", nameParts.Skip(1));
                }
            }

            if (!string.IsNullOrEmpty(userId))
            {
                var existingUser = await userDetailService.GetUserByUserIdAsync(userId);

                if (existingUser == null)
                {
                    var userDetailsDto = new UserDetailsDto
                    {
                        UserId = userId,
                        Email = email,
                        FirstName = firstName,
                        LastName = lastName,
                        Role = rolesClaim.FirstOrDefault() ?? "User"
                    };

                    await userDetailService.AddUserAsync(userDetailsDto);
                }
                else
                {

                    var identity = (ClaimsIdentity)context.User.Identity;
                    identity.AddClaim(new Claim(ClaimTypes.Role, existingUser.Role));
                }
            }
        }
    }
}
