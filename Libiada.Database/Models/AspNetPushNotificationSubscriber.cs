namespace Libiada.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Table for storing data about devices that are subscribers to push notifications.
/// </summary>
[Index("UserId", Name = "IX_AspNetPushNotificationSubscribers_UserId")]
[Index("UserId", "Endpoint", Name = "uk_AspNetPushNotificationSubscribers", IsUnique = true)]
[Comment("Table for storing data about devices that are subscribers to push notifications.")]
public partial class AspNetPushNotificationSubscriber
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Comment("Unique identifier.")]
    public int Id { get; set; }

    /// <summary>
    /// User id.
    /// </summary>
    [Comment("User id.")]
    public int UserId { get; set; }

    public string Endpoint { get; set; } = null!;

    public string P256dh { get; set; } = null!;

    public string Auth { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    [InverseProperty("AspNetPushNotificationSubscribers")]
    public virtual AspNetUser AspNetUser { get; set; } = null!;
}
