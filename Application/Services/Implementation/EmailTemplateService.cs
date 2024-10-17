using Application.Services.Abstraction;
using Core.Entities;

namespace Application.Services.Implementation
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private const string BaseStyle = @"
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
            a {{  
                color: #007bff;  
                text-decoration: none;  
            }}  
            a:hover {{  
                text-decoration: underline;  
            }}  
            .footer {{  
                margin-top: 20px;  
                font-size: 12px;  
                color: #777;  
            }}  
        </style>";

        public string GenerateNewRequestEmailHtml(PurchaseRequest purchaseRequest, string firstName, string lastName)
        {
            return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                {BaseStyle}
            </head>
            <body>
                <div class='container'>
                    <h1>New Item Request for Re-stock/Purchase</h1>
                    <p><strong>User ID:</strong> {purchaseRequest.UserId}</p>
                    <p><strong>User Full Name:</strong> {firstName} {lastName}</p>
                    <p><strong>Request Status:</strong> {purchaseRequest.Status}</p>
                    <p>Here are the details of the requested items:</p>
                    <ul>
                        {GenerateItemsListHtml(purchaseRequest.Items)}
                    </ul>
                    <p><strong>Created Date:</strong> {purchaseRequest.CreatedDate.ToString("u")}</p>
                    <p class='footer'>This is an automated message. Please do not reply.</p>
                </div>
            </body>
            </html>";
        }

        public string GenerateNewRequestEmailPlainText(PurchaseRequest purchaseRequest, string firstName, string lastName)
        {
            var items = string.Join("\n", purchaseRequest.Items.Select(item => $"{item.Name}: {item.Quantity}"));
            return $"New item request created by User Name: {firstName} {lastName}\n" +
                   $"Status: {purchaseRequest.Status}\n" +
                   $"Items:\n{items}\n" +
                   $"Created Date: {purchaseRequest.CreatedDate.ToString("u")}\n" +
                   $"This is an automated message. Please do not reply.";
        }

        public string GeneratePurchaseCompletedEmailHtml(PurchaseRequest request)
        {
            return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                {BaseStyle}
            </head>
            <body>
                <div class='container'>
                    <h1>Purchase Request Completed</h1>
                    <p>The following purchase request has been completed with receipts uploaded:</p>
                    <p><strong>User ID:</strong> {request.UserId}</p>
                    <p><strong>Items:</strong></p>
                    <ul>
                        {GenerateItemsListHtml(request.Items)}
                    </ul>
                    <p><strong>Receipt Images:</strong></p>
                    <ul>
                        {GenerateReceiptImagesHtml(request.ReceiptImageUrl)}
                    </ul>
                    <p><strong>Completed Date:</strong> {DateTime.UtcNow.ToString("u")}</p>
                    <p class='footer'>This is an automated message. Please do not reply.</p>
                </div>
            </body>
            </html>";
        }

        public string GeneratePurchaseCompletedEmailPlainText(PurchaseRequest request)
        {
            var items = string.Join("\n", request.Items.Select(item => $"{item.Name}: {item.Quantity}"));
            var receipts = string.Join("\n", request.ReceiptImageUrl.Select(file => $"{file.FileName}: {file.FileUrl}"));
            return $"Purchase request completed by User ID: {request.UserId}\n" +
                   $"Items:\n{items}\n" +
                   $"Receipt Images:\n{receipts}\n" +
                   $"Completed Date: {DateTime.UtcNow.ToString("u")}\n" +
                   $"This is an automated message. Please do not reply.";
        }

        public string GeneratePurchaseRejectedEmailHtml(string requestId)
        {
            return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                {BaseStyle}
            </head>
            <body>
                <div class='container'>
                    <h1>Purchase Request Rejected</h1>
                    <p>Your purchase request (ID: {requestId}) has been <strong>Rejected</strong>.</p>
                    <p>Updated Date: {DateTime.UtcNow.ToString("u")}</p>
                    <p class='footer'>This is an automated message. Please do not reply.</p>
                </div>
            </body>
            </html>";
        }

        public string GeneratePurchaseRejectedEmailPlainText(string requestId)
        {
            return $"Your purchase request (ID: {requestId}) has been Rejected.\n" +
                   $"Updated Date: {DateTime.UtcNow.ToString("u")}";
        }

        public string GeneratePurchaseApprovedEmailHtml(PurchaseRequest request)
        {
            return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                {BaseStyle}
            </head>
            <body>
                <div class='container'>
                    <h1>Purchase Request Approved</h1>
                    <p>Your purchase request (ID: {request.id}) has been <strong>Approved</strong>.</p>
                    <p>Updated Date: {DateTime.UtcNow.ToString("u")}</p>
                    <p class='footer'>This is an automated message. Please do not reply.</p>
                </div>
            </body>
            </html>";
        }

        public string GeneratePurchaseApprovedEmailPlainText(PurchaseRequest request)
        {
            return $"Your purchase request (ID: {request.id}) has been Approved.\n" +
                   $"Updated Date: {DateTime.UtcNow.ToString("u")}";
        }

        // Helper methods to generate HTML lists
        private string GenerateItemsListHtml(IEnumerable<Item> items)
        {
            return string.Join("", items.Select(item => $"<li>{item.Name}: {item.Quantity}</li>"));
        }

        private string GenerateReceiptImagesHtml(IEnumerable<FileEntity> files)
        {
            return string.Join("", files.Select(file => $"<li><a href='{file.FileUrl}'>{file.FileName}</a></li>"));
        }
    }
}
