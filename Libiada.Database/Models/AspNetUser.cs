namespace Libiada.Database.Models;

using Microsoft.AspNetCore.Identity;

using System.ComponentModel.DataAnnotations.Schema;

public class AspNetUser : IdentityUser<int>
{
    [InverseProperty("AspNetUser")]
    public virtual ICollection<AspNetPushNotificationSubscriber> AspNetPushNotificationSubscribers { get; set; } = new List<AspNetPushNotificationSubscriber>();

    [InverseProperty("AspNetUser")]
    public virtual ICollection<CalculationTask> CalculationTasks { get; set; } = new List<CalculationTask>();

    public virtual ICollection<AspNetRole> Roles { get; set; } = new List<AspNetRole>();
}
