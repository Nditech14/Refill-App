using Core.Entities;

namespace Application.Services.Abstraction
{
    public interface IEmailTemplateService
    {
        string GenerateNewRequestEmailHtml(PurchaseRequest purchaseRequest, string firstName, string lastName);
        string GenerateNewRequestEmailPlainText(PurchaseRequest purchaseRequest, string firstName, string lastName);

        string GeneratePurchaseCompletedEmailHtml(PurchaseRequest request);
        string GeneratePurchaseCompletedEmailPlainText(PurchaseRequest request);

        string GeneratePurchaseRejectedEmailHtml(string requestId);
        string GeneratePurchaseRejectedEmailPlainText(string requestId);

        string GeneratePurchaseApprovedEmailHtml(PurchaseRequest request);
        string GeneratePurchaseApprovedEmailPlainText(PurchaseRequest request);


    }
}
