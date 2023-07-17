using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPRM.Models.User
{
    [Table("asptnetUserPermissions")]
    public class UserPermission
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public string Permissions { get; set; }
    }
}
