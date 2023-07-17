namespace RPRM.Data
{
    using Microsoft.AspNetCore.Identity;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public static class RoleInitializer
    {
        public static async Task InitializeRoles(RoleManager<IdentityRole> roleManager)
        {
            var roles = new List<string>
        {
            "Admin",
            "Manager",
            "User"
        };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }

}
