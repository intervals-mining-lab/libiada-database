namespace Libiada.Database.Models;

using Microsoft.AspNetCore.Identity;

public class AspNetRole : IdentityRole<int>
{
    public virtual ICollection<AspNetUser> Users { get; set; } = new List<AspNetUser>();
}
