namespace Libiada.Database.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Web application user.
/// Extends <see cref="IdentityUser{int}"/>.
/// </summary>
[Comment("Web application user.")]
public class AspNetUser : IdentityUser<int>
{
    /// <summary>
    /// User creation date and time (filled trough trigger).
    /// </summary>
    [Column("created")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Comment("User creation date and time (filled trough trigger).")]
    public DateTimeOffset Created { get; private set; }

    /// <summary>
    /// User last change date and time (updated trough trigger).
    /// </summary>
    [Column("modified")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Comment("User last change date and time (updated trough trigger).")]
    public DateTimeOffset Modified { get; private set; }

    [InverseProperty("AspNetUser")]
    public virtual ICollection<AspNetPushNotificationSubscriber> AspNetPushNotificationSubscribers { get; set; } = [];

    [InverseProperty("AspNetUser")]
    public virtual ICollection<CalculationTask> CalculationTasks { get; set; } = [];
}
