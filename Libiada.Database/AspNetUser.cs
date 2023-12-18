using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;


namespace Libiada.Database
{
    public class AspNetUser : IdentityUser<int>
    {
        public virtual ICollection<AspNetRole> Roles { get; set; } = new List<AspNetRole>();
    }
}
