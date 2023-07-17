using Microsoft.AspNetCore.Mvc.Rendering;

namespace RPRM.Models.User
{
    public class UpdateUserAdmin
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Company { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
       
    }
}
