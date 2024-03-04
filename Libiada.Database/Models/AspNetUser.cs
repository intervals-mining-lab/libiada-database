namespace Libiada.Database.Models;

using Microsoft.AspNetCore.Identity;

using System.ComponentModel.DataAnnotations.Schema;

public class AspNetUser : IdentityUser<int>
{
    /// <summary>
    /// User creation date and time (filled trough trigger).
    /// </summary>
    [Column("created")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTimeOffset Created { get; private set; }

    /// <summary>
    /// User last change date and time (updated trough trigger).
    /// </summary>
    [Column("modified")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTimeOffset Modified { get; private set; }

    [InverseProperty("AspNetUser")]
    public virtual ICollection<AspNetPushNotificationSubscriber> AspNetPushNotificationSubscribers { get; set; } = new List<AspNetPushNotificationSubscriber>();

    [InverseProperty("AspNetUser")]
    public virtual ICollection<CalculationTask> CalculationTasks { get; set; } = new List<CalculationTask>();

    public virtual ICollection<AspNetRole> Roles { get; set; } = new List<AspNetRole>();
}
