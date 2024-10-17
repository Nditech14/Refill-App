using Application.Dtos;
using Application.Services.Abstraction;
using AutoMapper;
using Core.Entities;
using Core.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RequestItemController : ControllerBase
    {
        private readonly IRequestItemService _requestItemService;
        private readonly IMapper _mapper;

        public RequestItemController(IRequestItemService requestItemService, IMapper mapper)
        {
            _requestItemService = requestItemService;
            _mapper = mapper;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateRequest([FromBody] ItemRequestForDto requestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {

                var requestEntity = _mapper.Map<ItemRequestFor>(requestDto);

                var createdRequest = await _requestItemService.CreateRequestAsync(requestEntity);


                return CreatedAtAction(nameof(GetRequest), new { id = createdRequest.id }, createdRequest);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllRequests()
        {
            try
            {
                var requests = await _requestItemService.GetAllRequestsAsync();



                return Ok(requests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetRequest(string id)
        {
            try
            {
                var request = await _requestItemService.GetRequestAsync(id);

                if (request == null)
                    return NotFound();


                var requestDto = _mapper.Map<ItemRequestForDto>(request);

                return Ok(requestDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateRequestStatus(string id, [FromBody] RequestStatus newStatus)
        {
            if (!Enum.IsDefined(typeof(RequestStatus), newStatus))
                return BadRequest("Invalid status value");

            try
            {
                var updated = await _requestItemService.UpdateRequestStatusAsync(id, newStatus);

                if (!updated)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRequest(string id)
        {
            try
            {
                var deleted = await _requestItemService.DeleteRequestAsync(id);

                if (!deleted)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
