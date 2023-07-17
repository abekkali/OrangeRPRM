using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace RPRM.Models.User
{
    public class User :IdentityUser
    {
        
        public string? FullName { get; set; }

       
        public string? Company { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; } 

        public virtual ICollection<IdentityUserClaim<string>> Claims { get; set; }

    }

    }

