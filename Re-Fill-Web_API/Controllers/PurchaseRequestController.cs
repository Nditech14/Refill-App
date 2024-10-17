using Application.Dtos;
using Application.ResponseDto;
using Application.Services.Abstraction;
using Core;
using Core.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseRequestController : ControllerBase
    {
        private readonly IPurchaseRequestService _purchaseRequestService;

        public PurchaseRequestController(IPurchaseRequestService purchaseRequestService)
        {
            _purchaseRequestService = purchaseRequestService;
        }


        [HttpGet("load-more")]
        public async Task<IActionResult> GetPaginatedPurchaseRequests([FromQuery] string continuationToken = null, [FromQuery] int pageSize = 10)
        {
            var (requests, nextContinuationToken) = await _purchaseRequestService.GetPaginatedPurchaseRequestsAsync(continuationToken, pageSize);
            var response = new
            {
                Items = requests,
                ContinuationToken = nextContinuationToken
            };

            return Ok(new ApiResponse<object>(response, "Purchase requests retrieved successfully"));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("status/load-more")]
        public async Task<IActionResult> GetPaginatedPurchaseRequestsByStatus([FromQuery] RequestStatus status, [FromQuery] string continuationToken = null, [FromQuery] int pageSize = 10)
        {

            var (requests, nextContinuationToken) = await _purchaseRequestService.LoadMorePurchaseRequestsByStatusAsync(status, continuationToken, pageSize);


            var response = new
            {
                Items = requests,
                ContinuationToken = nextContinuationToken
            };


            return Ok(new ApiResponse<object>(response, $"Purchase requests with status '{status}' retrieved successfully"));
        }


        [HttpPost("create-request")]
        public async Task<IActionResult> CreatePurchaseRequest([FromBody] PurchaseRequestDto purchaseRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>("Invalid data."));
            }

            var newRequest = await _purchaseRequestService.CreatePurchaseRequestAsync(purchaseRequestDto);
            return CreatedAtAction(nameof(GetPurchaseRequestById), new { id = newRequest.id }, new ApiResponse<object>(newRequest, "Purchase request created successfully"));
        }


        [HttpGet("{id}/get-request-by")]
        public async Task<IActionResult> GetPurchaseRequestById(string id)
        {
            var purchaseRequest = await _purchaseRequestService.GetPurchaseRequestAsync(id);
            if (purchaseRequest == null)
            {
                return NotFound(new ApiResponse<object>("Purchase request not found."));
            }

            return Ok(new ApiResponse<object>(purchaseRequest, "Purchase request retrieved successfully."));
        }


        [HttpPut("{id}/upload-receipt")]
        public async Task<IActionResult> EditPurchaseRequest(string id, [FromForm] List<IFormFile> receiptImages)
        {
            try
            {
                var updatedRequest = await _purchaseRequestService.EditPurchaseRequestAsync(id, receiptImages);
                return Ok(new ApiResponse<object>(updatedRequest, "Purchase request updated successfully."));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiResponse<object>("Purchase request not found."));

            }
        }

        [HttpPut("update-purchased-items/{id}")]
        public async Task<IActionResult> EditRequest(string id, [FromBody] UpdatePurchasedItemsDto purchaseRequestDto)
        {
            try
            {
                var result = await _purchaseRequestService.PurChasedItems(id, purchaseRequestDto);
                return Ok(new ApiResponse<object>(result, "PurchasedItem request updated successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing the request.", details = ex.Message });
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/update-request-status")]
        public async Task<IActionResult> UpdatePurchaseRequestStatus(string id, [FromQuery] RequestStatus status)
        {
            try
            {
                var result = await _purchaseRequestService.UpdatePurchaseRequestStatusAsync(id, status);
                return Ok(new ApiResponse<object>(result, "Purchase request status updated successfully."));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiResponse<object>("Purchase request not found."));
            }
        }


        [HttpDelete("delete-request/{id}")]
        public async Task<IActionResult> DeletePurchaseRequest(string id)
        {
            var success = await _purchaseRequestService.DeletePurchaseRequestAsync(id);
            if (!success)
            {
                return NotFound(new ApiResponse<object>("Purchase request not found."));
            }

            return Ok(new ApiResponse<object>(true, "Purchase request deleted successfully."));
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("date-range")]
        public async Task<IActionResult> GetPurchaseRequestsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {

            if (startDate > endDate)
            {
                return BadRequest(new ApiResponse<object>("Start date cannot be later than end date."));
            }

            var requests = await _purchaseRequestService.GetPurchaseRequestsByDateRangeAsync(startDate, endDate);

            if (requests == null || !requests.Any())
            {
                return NotFound(new ApiResponse<object>("No purchase requests found in the specified date range."));
            }

            return Ok(new ApiResponse<IEnumerable<PurchaseRequestResponseDto>>(requests, "Purchase requests retrieved successfully."));
        }
    }
}
