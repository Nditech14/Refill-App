namespace Application.Messaging
{
    public class AdminNotificationMessage
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public string AdminEmail { get; set; }
    }
}
