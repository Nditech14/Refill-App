using Application.Services.Abstraction;
using AutoMapper;
using Core.Entities;
using Core.Enum;
using Microsoft.AspNetCore.Http;

namespace Application.Services.Implementation
{
    public class RequestItemService : IRequestItemService
    {
        private readonly ICosmosDbService<ItemRequestFor> _cosmosDbService;
        private readonly IInventoryService _inventoryService;
        private readonly ICosmosDbService<Inventory> _cosmosInventory;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;
        private readonly IUserDetailService _userDetailService;

        public RequestItemService(
            ICosmosDbService<ItemRequestFor> cosmosDbService,
            IInventoryService inventoryService,
            IMapper mapper,
            ICosmosDbService<Inventory> cosmosInventory,
            IHttpContextAccessor httpContextAccessor,
            IEmailService emailService,
            IUserDetailService userDetailService)
        {
            _cosmosDbService = cosmosDbService;
            _inventoryService = inventoryService;
            _mapper = mapper;
            _cosmosInventory = cosmosInventory;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
            _userDetailService = userDetailService;
        }

        public async Task<ItemRequestFor> CreateRequestAsync(ItemRequestFor request)
        {
            request.CreatedDate = DateTime.UtcNow;
            request.Status = RequestStatus.Pending;
            await _cosmosDbService.AddItemAsync(request, request.id);

            // Notify Admins about the new request
            var adminEmails = await _userDetailService.GetAdminEmailsAsync();
            string subject = "New Item Request Notification";
            string htmlContent = $"<h1>New Item Request</h1><p>A new item request has been created with the following details:</p>" +
                                 $"<ul>{string.Join("", request.Items.Select(item => $"<li>{item.Name}: {item.Quantity}</li>"))}</ul>";
            string plainTextContent = "A new item request has been created.";

            // Send email to each admin
            foreach (var email in adminEmails)
            {
                await _emailService.SendEmailAsync(email, subject, htmlContent, plainTextContent);
            }

            return request;
        }

        public async Task<bool> UpdateRequestStatusAsync(string requestId, RequestStatus newStatus)
        {
            // Retrieve the request from the database
            var request = await _cosmosDbService.GetItemAsync(requestId, requestId);
            if (request == null)
                return false;

            // Update request status
            request.Status = newStatus;

            // Notify the user if the request is approved or rejected
            if (newStatus == RequestStatus.Approved || newStatus == RequestStatus.Rejected)
            {
                await NotifyUserAboutRequestStatus(request, newStatus);
            }

            // Regenerate email content with the updated status
            var userEmails = await _userDetailService.GetUserEmailsAsync();
            string subject = "Item Request Notification";

            // Regenerate the email content to reflect the updated status
            string htmlContent = $"<h1>Item Requested</h1><p>Your request has been {request.Status} with the following details:</p>" +
                                 $"<ul>{string.Join("", request.Items.Select(item => $"<li>{item.Name}: {item.Quantity}</li>"))}</ul>";

            string plainTextContent = $"Your request has been {request.Status}";

            // Send email to each admin
            foreach (var email in userEmails)
            {
                await _emailService.SendEmailAsync(email, subject, htmlContent, plainTextContent);
            }

            // If the request is approved, handle the approval process
            if (newStatus == RequestStatus.Approved)
            {
                var success = await HandleApprovedRequestAsync(request);
                if (!success)
                {
                    return false; // If handling the approved request fails, return false
                }
            }

            // Update the request in the database after handling the approval
            await _cosmosDbService.UpdateItemAsync(requestId, request, request.id);
            return true; // Return true to indicate that the status update was successful
        }


        private async Task<bool> HandleApprovedRequestAsync(ItemRequestFor request)
        {
            foreach (var item in request.Items)
            {
                var inventoryQuery = $"SELECT * FROM c WHERE c.Name = '{item.Name}'";
                var inventoryItem = (await _cosmosInventory.GetItemsAsync(inventoryQuery)).FirstOrDefault();

                if (inventoryItem == null)
                    return false;

                if (inventoryItem.Quantity < item.Quantity)
                    return false;

                inventoryItem.Quantity -= item.Quantity;
                await _cosmosInventory.UpdateItemAsync(inventoryItem.id, inventoryItem, inventoryItem.id);
            }

            return true;
        }

        private async Task NotifyUserAboutRequestStatus(ItemRequestFor request, RequestStatus status)
        {
            // Get the user who created the request (assuming request contains UserId)
            var user = await _userDetailService.GetUserByUserIdAsync(request.id);

            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                string subject = $"Your Request has been {status}";
                string htmlContent = $"<p>Your request for the following items has been {status}:</p>" +
                                     $"<ul>{string.Join("", request.Items.Select(item => $"<li>{item.Name}: {item.Quantity}</li>"))}</ul>";
                string plainTextContent = $"Your request has been {status}.";

                await _emailService.SendEmailAsync(user.Email, subject, htmlContent, plainTextContent);
            }
        }

        public async Task<IEnumerable<ItemRequestFor>> GetAllRequestsAsync()
        {
            var query = "SELECT * FROM c";
            return await _cosmosDbService.GetItemsAsync(query);
        }

        public async Task<ItemRequestFor> GetRequestAsync(string id)
        {
            return await _cosmosDbService.GetItemAsync(id, id);
        }

        public async Task<bool> DeleteRequestAsync(string id)
        {
            var request = await _cosmosDbService.GetItemAsync(id, id);
            if (request == null)
                return false;

            await _cosmosDbService.DeleteItemAsync(id, id);
            return true;
        }
    }
}
