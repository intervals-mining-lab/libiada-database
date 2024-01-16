namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Table for storing data about devices that are subscribers to push notifications.
/// </summary>
[Index("UserId", Name = "IX_AspNetPushNotificationSubscribers_UserId")]
[Index("UserId", "Endpoint", Name = "uk_AspNetPushNotificationSubscribers", IsUnique = true)]
public partial class AspNetPushNotificationSubscriber
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Endpoint { get; set; } = null!;

    public string P256dh { get; set; } = null!;

    public string Auth { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("AspNetPushNotificationSubscribers")]
    public virtual AspNetUser AspNetUser { get; set; } = null!;
}
