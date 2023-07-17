using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RPRM.Models;
using RPRM.Models.User;

namespace RPRM.Data
{
    public class AuthorizePermissionAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        public string ClassName { get; set; }
        public string PermissionType { get; set; }

        public AuthorizePermissionAttribute(string className, string permissionType)
        {
            ClassName = className;
            PermissionType = permissionType;
        }

        public async void OnAuthorization(AuthorizationFilterContext context)
        {
            var userManager = context.HttpContext.RequestServices.GetService<UserManager<User>>();

            if (userManager == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            var user = await userManager.GetUserAsync(context.HttpContext.User);
            if (user == null)
            {
                context.Result = new CustomResult("Erreur utilisateur");
                return;
            }
            // Vérifier les permissions spécifiques uniquement si l'utilisateur n'a pas les rôles "Admin" ou "Manager".
            if (!(await userManager.IsInRoleAsync(user, "Admin") || await userManager.IsInRoleAsync(user, "Manager")))
            {
                var permissionHandler = context.HttpContext.RequestServices.GetService<PermissionHandler>();
                if (permissionHandler == null)
                {
                    context.Result = new CustomResult("Erreur Permission");
                    return;
                }
                // Si l'utilisateur n'a pas la permission requise, afficher un message d'erreur personnalisé
                if (!await permissionHandler.HasPermissionAsync(user.Id, ClassName, PermissionType))
                {
                    string permissionName = PermissionType == "view" ? "lecture" : "modification";
                    context.Result = new CustomResult($"Vous avez besoin de la permission {permissionName} pour {ClassName}.");
                }
            }
        }
    }

}
