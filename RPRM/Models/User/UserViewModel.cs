namespace RPRM.Models.User
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email {get; set; }
        public string PhoneNumber { get; set; }
        public string Company { get; set; }

        public string Role { get; set; }
        public bool EmailConfirmed { get; set; }
        public string LockoutEnd { get; set; }
        public bool IsAccessRestricted { get; set; }
        public string AccessEndTime { get; set; }
    }
}
