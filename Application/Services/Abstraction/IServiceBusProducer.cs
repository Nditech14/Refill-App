using Application.Messaging;

namespace Application.Services.Abstraction
{
    public interface IServiceBusProducer
    {
        Task SendAdminNotificationAsync(AdminNotificationMessage message);
        Task SendUserNotificationAsync(UserNotificationMessage message);
    }
}
