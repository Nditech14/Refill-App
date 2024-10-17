using Application.Dtos;
using Application.Services.Abstraction;
using Core;
using Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Re_Fill_Web_API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;

        }




        [HttpGet]
        public async Task<IActionResult> GetAllInventory()
        {

            try
            {
                var inventories = await _inventoryService.GetAllInventoryAsync();
                return Ok(new ApiResponse<IEnumerable<Inventory>>(inventories, "Inventory items retrieved successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<string>(null, ex.Message));
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetInventoryById(string id)
        {

            try
            {
                var inventory = await _inventoryService.GetInventoryAsync(id);

                if (inventory == null)
                    return NotFound(new ApiResponse<string>(null, "Inventory item not found."));

                return Ok(new ApiResponse<Inventory>(inventory, "Inventory item retrieved successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<string>(null, ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddInventory([FromBody] InventoryDto inventoryDto)
        {


            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<string>(null, "Invalid input data."));

            try
            {
                var inventory = await _inventoryService.AddInventoryAsync(inventoryDto);
                return CreatedAtAction(nameof(GetInventoryById), new { id = inventory.id }, new ApiResponse<Inventory>(inventory, "Inventory item created/updated successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<string>(null, ex.Message));
            }
        }


        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStockItems([FromQuery] int threshold = 3)
        {
            try
            {
                var lowStockItems = await _inventoryService.GetLowStockItemsAsync(threshold);
                return Ok(new ApiResponse<IEnumerable<Inventory>>(lowStockItems, "Low stock items retrieved successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<string>(null, ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("take/{id}")]
        public async Task<IActionResult> TakeInventoryItem(string id, [FromBody] int quantityToTake)
        {

            if (quantityToTake <= 0)
                return BadRequest(new ApiResponse<string>(null, "Quantity must be greater than zero."));

            try
            {
                var result = await _inventoryService.TakeInventoryItemAsync(id, quantityToTake);

                if (result)
                    return Ok(new ApiResponse<bool>(true, "Inventory quantity updated successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<string>(null, ex.Message));
            }

            return BadRequest(new ApiResponse<bool>(false, "Unable to update inventory."));
        }

        [HttpGet("loadMore")]
        public async Task<IActionResult> LoadMoreInventories([FromQuery] string continuationToken = null, [FromQuery] int? pageSize = null)
        {
            try
            {

                var result = await _inventoryService.LoadMoreInventoryAsync(continuationToken, pageSize);


                return Ok(new
                {
                    Items = result.Items,
                    ContinuationToken = result.ContinuationToken
                });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { Message = "An error occurred while fetching inventory items.", Details = ex.Message });
            }
        }
    }
}
