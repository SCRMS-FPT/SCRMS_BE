using System.ComponentModel.DataAnnotations;

namespace Notification.API.Data.Model
{
    public class MessageNotification
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid Receiver { get; set; }
        public Boolean IsRead { get; set; } = false;
        public string Title { get; set; }
        public string Content { get; set; }
        public string Type { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
