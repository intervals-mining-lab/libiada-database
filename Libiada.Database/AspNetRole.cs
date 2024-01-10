using Microsoft.AspNetCore.Identity;


namespace Libiada.Database;

public class AspNetRole : IdentityRole<int> 
{
    public virtual ICollection<AspNetUser> Users { get; set; } = new List<AspNetUser>();
}
