using Application.Dtos;
using Application.ResponseDto;
using Application.Services.Abstraction;
using AutoMapper;
using Core.Entities;
using Core.Enum;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Application.Services.Implementation
{
    public class PurchaseRequestService : IPurchaseRequestService
    {
        private readonly ICosmosDbService<PurchaseRequest> _cosmosDbService;
        private readonly IFileService<FileEntity> _fileService;
        private readonly IInventoryService _inventoryService;
        private readonly IMapper _mapper;
        private const int DefaultPageSize = 10;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;
        private readonly IUserDetailService _userDetailService;
        private readonly IEmailTemplateService _emailTemplateService;

        public PurchaseRequestService(
            ICosmosDbService<PurchaseRequest> cosmosDbService,
            IFileService<FileEntity> fileService,
            IInventoryService inventoryService,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IEmailService emailService,
            IUserDetailService userDetailService,
            IEmailTemplateService emailTemplateService)
        {
            _cosmosDbService = cosmosDbService;
            _fileService = fileService;
            _inventoryService = inventoryService;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
            _userDetailService = userDetailService;
            _emailTemplateService = emailTemplateService;
        }

        public async Task<PurchaseRequest> CreatePurchaseRequestAsync(PurchaseRequestDto purchaseRequestDto)
        {
            var purchaseRequest = _mapper.Map<PurchaseRequest>(purchaseRequestDto);
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            purchaseRequest.UserId = userId;
            purchaseRequest.Status = RequestStatus.Pending;
            purchaseRequest.CreatedDate = DateTime.UtcNow;

            await _cosmosDbService.AddItemAsync(purchaseRequest);

            var firstName = await _userDetailService.GetUserFirstNameByIdAsync(userId);
            var lastName = await _userDetailService.GetUserLastNameByIdAsync(userId);
            var adminEmails = await _userDetailService.GetAdminEmailsAsync();
            string subject = "New Item Request for Re-stock/Purchase";


            string htmlContent = _emailTemplateService.GenerateNewRequestEmailHtml(purchaseRequest, firstName, lastName);
            string plainTextContent = _emailTemplateService.GenerateNewRequestEmailPlainText(purchaseRequest, firstName, lastName);

            foreach (var email in adminEmails)
            {
                await _emailService.SendEmailAsync(email, subject, htmlContent, plainTextContent);
            }

            return purchaseRequest;
        }

        public async Task<PurchaseRequestResponseDto> EditPurchaseRequestAsync(string id, List<IFormFile> receiptImages)
        {
            var request = await _cosmosDbService.GetItemAsync(id, id);
            if (request == null)
                throw new KeyNotFoundException("Purchase request not found");
            if (request.Status != RequestStatus.Approved)
            {
                throw new InvalidOperationException("Item request is yet to be approved.");
            }

            if (request.ReceiptImageUrl == null)
            {
                request.ReceiptImageUrl = new List<FileEntity>();
            }

            foreach (var image in receiptImages)
            {
                using var fileStream = image.OpenReadStream();
                var uploadedFile = await _fileService.UploadFileAsync(fileStream, image.FileName);
                request.ReceiptImageUrl.Add(uploadedFile);
            }

            request.Status = RequestStatus.Purchased;
            await _cosmosDbService.UpdateItemAsync(request.id, request, request.id);

            var adminEmails = await _userDetailService.GetAdminEmailsAsync();
            string subject = "Purchase Request Completed - Receipt Uploaded";


            string htmlContent = _emailTemplateService.GeneratePurchaseCompletedEmailHtml(request);
            string plainTextContent = _emailTemplateService.GeneratePurchaseCompletedEmailPlainText(request);

            foreach (var email in adminEmails)
            {
                await _emailService.SendEmailAsync(email, subject, htmlContent, plainTextContent);
            }

            return _mapper.Map<PurchaseRequestResponseDto>(request);
        }
        public async Task<bool> UpdatePurchaseRequestStatusAsync(string id, RequestStatus status)
        {
            var request = await _cosmosDbService.GetItemAsync(id, id);
            if (request == null)
            {
                throw new KeyNotFoundException("Purchase request not found");
            }

            var userEmail = await _userDetailService.GetUserEmailByIdAsync(request.UserId);
            if (string.IsNullOrEmpty(userEmail))
            {
                throw new Exception("User email not found for the request");
            }

            if (status == RequestStatus.Rejected)
            {
                await _cosmosDbService.DeleteItemAsync(id, id);


                string subject = "Your Purchase Request Status Update - Rejected";
                string htmlContent = _emailTemplateService.GeneratePurchaseRejectedEmailHtml(request.id);
                string plainTextContent = _emailTemplateService.GeneratePurchaseRejectedEmailPlainText(request.id);

                await _emailService.SendEmailAsync(userEmail, subject, htmlContent, plainTextContent);
                return true;
            }

            if (status == RequestStatus.Completed)
            {
                foreach (var item in request.Items)
                {
                    var inventoryItem = await _inventoryService.GetInventoryAsync(item.Name);
                    if (inventoryItem != null)
                    {
                        inventoryItem.Quantity += item.Quantity;
                        await _inventoryService.TakeInventoryItemAsync(inventoryItem.id, item.Quantity);
                    }
                    else
                    {
                        var newInventoryItem = new Inventory
                        {
                            Name = item.Name,
                            Quantity = item.Quantity,
                            Description = item.Description
                        };
                        await _inventoryService.AddInventoryAsync(_mapper.Map<InventoryDto>(newInventoryItem));
                    }
                }


                string subject = "Your Purchase Request Status Update - Completed";
                string htmlContent = _emailTemplateService.GeneratePurchaseCompletedEmailHtml(request);
                string plainTextContent = _emailTemplateService.GeneratePurchaseCompletedEmailPlainText(request);

                await _emailService.SendEmailAsync(userEmail, subject, htmlContent, plainTextContent);
            }


            request.Status = status;
            await _cosmosDbService.UpdateItemAsync(request.id, request, request.id);

            if (status == RequestStatus.Approved)
            {

                string subject = "Your Purchase Request Status Update - Approved";
                string htmlContent = _emailTemplateService.GeneratePurchaseApprovedEmailHtml(request);
                string plainTextContent = _emailTemplateService.GeneratePurchaseApprovedEmailPlainText(request);

                await _emailService.SendEmailAsync(userEmail, subject, htmlContent, plainTextContent);
            }

            return true;
        }

        public async Task<bool> DeletePurchaseRequestAsync(string id)
        {
            var request = await _cosmosDbService.GetItemAsync(id, id);
            if (request == null)
                return false;

            await _cosmosDbService.DeleteItemAsync(id, id);
            return true;
        }

        public async Task<PurchaseRequestResponseDto> GetPurchaseRequestAsync(string id)
        {
            var request = await _cosmosDbService.GetItemAsync(id, id);
            return _mapper.Map<PurchaseRequestResponseDto>(request);
        }

        public async Task<IEnumerable<PurchaseRequestResponseDto>> GetAllPurchaseRequestsAsync()
        {
            var query = "SELECT * FROM c";
            var requests = await _cosmosDbService.GetItemsAsync(query);
            return _mapper.Map<IEnumerable<PurchaseRequestResponseDto>>(requests);
        }

        public async Task<IEnumerable<PurchaseRequest>> GetPurchaseRequestsByStatusAsync(RequestStatus status)
        {
            var query = $"SELECT * FROM c WHERE c.Status = '{status}'";
            var requests = await _cosmosDbService.GetItemsAsync(query);
            return requests;
        }

        public async Task<IEnumerable<PurchaseRequestResponseDto>> GetPurchaseRequestsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var query = $"SELECT * FROM c WHERE c.CreatedDate >= '{startDate:O}' AND c.CreatedDate <= '{endDate:O}'";
            var requests = await _cosmosDbService.GetItemsAsync(query);
            return _mapper.Map<IEnumerable<PurchaseRequestResponseDto>>(requests);
        }

        public async Task<(IEnumerable<PurchaseRequestResponseDto> Items, string ContinuationToken)> GetPaginatedPurchaseRequestsAsync(string continuationToken, int pageSize = DefaultPageSize)
        {
            var result = await _cosmosDbService.GetItemsWithContinuationTokenAsync(continuationToken, pageSize);
            var purchaseRequestDtos = _mapper.Map<IEnumerable<PurchaseRequestResponseDto>>(result.Items);
            return (purchaseRequestDtos, result.ContinuationToken);
        }

        public async Task<(IEnumerable<PurchaseRequestResponseDto> Items, string ContinuationToken)> GetPaginatedPurchaseRequestsByStatusAsync(RequestStatus status, string continuationToken, int pageSize = DefaultPageSize)
        {
            var query = $"SELECT * FROM c WHERE c.Status = {(int)status}";
            var result = await _cosmosDbService.GetItemsWithContinuationTokenAsync(continuationToken, pageSize, query);
            var purchaseRequestDtos = _mapper.Map<IEnumerable<PurchaseRequestResponseDto>>(result.Items);
            return (purchaseRequestDtos, result.ContinuationToken);
        }

        public async Task<(IEnumerable<PurchaseRequest> Items, string ContinuationToken)> LoadMorePurchaseRequestsByStatusAsync(RequestStatus status, string continuationToken, int pageSize = DefaultPageSize)
        {
            var query = $"SELECT * FROM c WHERE c.Status = {(int)status}";


            var result = await _cosmosDbService.GetItemsWithContinuationTokenAsynczz(continuationToken, pageSize, query);


            var purchaseRequests = _mapper.Map<IEnumerable<PurchaseRequest>>(result.Items);


            return (purchaseRequests, result.ContinuationToken);
        }

        public async Task<PurchaseRequestResponseDto> PurChasedItems(string id, UpdatePurchasedItemsDto purchaseRequestDto)
        {
            var request = await _cosmosDbService.GetItemAsync(id, id);
            if (request == null)
                throw new KeyNotFoundException("Purchase request not found");

            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || request.UserId != userId)
                throw new UnauthorizedAccessException("User is not authenticated or does not have access to edit this request.");

            if (request.Status != RequestStatus.Approved)
            {
                throw new InvalidOperationException("Item request is yet to be approved.");
            }

            request.Items = purchaseRequestDto.Items.Select(itemDto => _mapper.Map<Item>(itemDto)).ToList();

            request.Status = RequestStatus.Approved;
            request.CreatedDate = DateTime.UtcNow;


            await _cosmosDbService.UpdateItemAsync(request.id, request, request.id);

            var adminEmails = await _userDetailService.GetAdminEmailsAsync();
            string subject = "Purchase Request Updated - Items and Receipt Uploaded";

            string htmlContent = $@"  
<!DOCTYPE html>  
<html lang='en'>  
<head>  
    <meta charset='UTF-8'>  
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>  
    <style>  
        body {{  
            font-family: Arial, sans-serif;  
            background-color: #f4f4f4;  
            margin: 0;  
            padding: 20px;  
        }}  
        .container {{  
            background-color: #fff;  
            border-radius: 5px;  
            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);  
            padding: 20px;  
        }}  
        h1 {{  
            color: #333;  
        }}  
        p {{  
            color: #555;  
        }}  
        ul {{  
            list-style-type: none;  
            padding: 0;  
        }}  
        li {{  
            background: #f0f0f0;  
            margin: 5px 0;  
            padding: 10px;  
            border-radius: 3px;  
        }}  
        .footer {{  
            margin-top: 20px;  
            font-size: 12px;  
            color: #777;  
        }}  
    </style>  
</head>  
<body>  
    <div class='container'>  
        <h1>Purchase Request Updated</h1>  
        <p><strong>User ID:</strong> {request.UserId}</p>  
        <p><strong>Status:</strong> {request.Status}</p>  
        <p><strong>Updated Items:</strong></p>  
        <ul>  
            {string.Join("", request.Items.Select(item => $"<li>{item.Name}: {item.Quantity}</li>"))}  
        </ul>  
        <p><strong>Last Updated Date:</strong> {DateTime.UtcNow.ToString("u")}</p>  
        <p class='footer'>This is an automated message. Please do not reply.</p>  
    </div>  
</body>  
</html>";

            string plainTextContent = $"Purchase request updated by User ID: {request.UserId}\n" +
                                      $"Status: {request.Status}\n" +
                                      $"Items:\n" +
                                      $"{string.Join("\n", request.Items.Select(item => $"{item.Name}: {item.Quantity}"))}\n" +
                                      $"Last Updated Date: {DateTime.UtcNow.ToString("u")}\n" +
                                      $"This is an automated message. Please do not reply.";


            foreach (var email in adminEmails)
            {
                await _emailService.SendEmailAsync(email, subject, htmlContent, plainTextContent);
            }

            return _mapper.Map<PurchaseRequestResponseDto>(request);
        }


    }
}
