using Application.Dtos;
using Application.Services.Abstraction;
using AutoMapper;
using Core;
using Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserDetailController : ControllerBase
    {
        private readonly IUserDetailService _userDetailService;
        private readonly IMapper _mapper;

        public UserDetailController(IUserDetailService userDetailService, IMapper mapper)
        {
            _userDetailService = userDetailService;
            _mapper = mapper;
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("userdetail/{id}")]
        public async Task<ActionResult<ApiResponse<UserDetails>>> GetUserById(string id)
        {
            var user = await _userDetailService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound(new ApiResponse<UserDetails>($"User with id {id} not found"));
            }

            return Ok(new ApiResponse<UserDetails>(user, "User fetched successfully"));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("get-all-user")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserDetails>>>> GetAllUsers()
        {
            var users = await _userDetailService.GetAllUsersAsync();
            return Ok(new ApiResponse<IEnumerable<UserDetails>>(users, "Users fetched successfully"));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create-user")]
        public async Task<ActionResult<ApiResponse<UserDetails>>> CreateUser(UserDetailsDto userDetails)
        {
            try
            {
                await _userDetailService.AddUserAsync(userDetails);
                return CreatedAtAction(nameof(GetUserById), new { id = userDetails }, new ApiResponse<UserDetailsDto>(userDetails, "User created successfully"));
            }
            catch (System.Exception ex)
            {
                return BadRequest(new ApiResponse<UserDetails>(ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update-user/{id}")]
        public async Task<ActionResult<ApiResponse<UserDetails>>> UpdateUser(string id, UserDetailsDto userDetailsDto)
        {
            var userDetails = _mapper.Map<UserDetails>(userDetailsDto);
            if (id != userDetails.id)
            {
                return BadRequest(new ApiResponse<UserDetails>("ID mismatch"));
            }

            await _userDetailService.UpdateUserAsync(id, userDetailsDto);
            return Ok(new ApiResponse<UserDetails>(userDetails, "User updated successfully"));
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteUser(string id)
        {
            await _userDetailService.DeleteUserAsync(id);
            return Ok(new ApiResponse<string>("User deleted successfully"));
        }
    }
}
