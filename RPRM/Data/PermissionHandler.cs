using Microsoft.AspNetCore.Identity;
using RPRM.Models;
using RPRM.Models.User;
using System.Text.Json;

namespace RPRM.Data
{
    public class PermissionHandler
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public PermissionHandler(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<bool> HasPermissionAsync(string userId, string className, string permissionType)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            var userPermission = _context.UserPermissions.FirstOrDefault(p => p.UserId == user.Id);
            if (userPermission == null)
            {
                return false;
            }

            var permissions = JsonSerializer.Deserialize<List<Permission>>(userPermission.Permissions);
            var permission = permissions.FirstOrDefault(p => p.ClassName == className);

            if (permission == null)
            {
                return false;
            }

            if (permissionType == "view")
            {
                return permission.CanView;
            }
            else if (permissionType == "edit")
            {
                return permission.CanEdit;
            }

            return false;
        }

        public async Task<bool> CheckPermission(User user, string className, string permissionType)
        {
            if (await _userManager.IsInRoleAsync(user, "Admin") || await _userManager.IsInRoleAsync(user, "Manager"))
            {
                // Autorise l'accès pour les administrateurs et Manager
                return true;
            }
            else
            {
                return await HasPermissionAsync(user.Id, className, permissionType);
            }
        }
    }
}
