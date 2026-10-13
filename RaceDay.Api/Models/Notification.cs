namespace RaceDay.Api.Models;

public class Notification
{
    public int NotificationId { get; set; }
    public int UserId { get; set; }
    public int? EventId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; }

    public User User { get; set; } = null!;
    public Event? Event { get; set; }
}
