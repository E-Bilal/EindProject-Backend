using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendAPI.Model
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Friend> User { get; set; }
        public ICollection<Friend> Friend { get; set; }

        public string RoleRequest { get; set; }
    }
}
