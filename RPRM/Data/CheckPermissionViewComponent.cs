using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RPRM.Models.User;

namespace RPRM.Data
{
    public class CheckPermissionViewComponent : ViewComponent
    {
        private readonly UserManager<User> _userManager;
        private readonly PermissionHandler _permissionHandler;
        private readonly ILogger<CheckPermissionViewComponent> _logger;


        public CheckPermissionViewComponent(UserManager<User> userManager, PermissionHandler permissionHandler, ILogger<CheckPermissionViewComponent> logger)
        {
            _userManager = userManager;
            _permissionHandler = permissionHandler;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(string className = null, List<string> classNames = null)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
        
            if (classNames == null)
            {
                bool hasViewPermission = false;
                bool hasEditPermission = false;

                if (user != null)
                {
                    hasViewPermission = await _permissionHandler.CheckPermission(user, className, "view");
                    hasEditPermission = await _permissionHandler.CheckPermission(user, className, "edit");
                }

                _logger.LogInformation($"User {user.UserName} - View Permission: {hasViewPermission}, Edit Permission: {hasEditPermission}");

                TempData["Permissions"] = new { View = hasViewPermission, Edit = hasEditPermission };
            }
            else
            {
                Dictionary<string, bool> viewPermissions = new Dictionary<string, bool>();

                if (user != null)
                {
                    foreach (var cl in classNames)
                    {
                        bool hasViewPermission = await _permissionHandler.CheckPermission(user, cl, "view");
                        viewPermissions[cl] = hasViewPermission;
                    }
                }

                TempData["ViewPermissions"] = viewPermissions;
            }
            return Content(string.Empty);
        }

    }
}
