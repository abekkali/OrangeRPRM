using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RPRM.Data;
using RPRM.Models;
using RPRM.Models.User;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;

namespace RPRM.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UsersController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<User> _signInManager;


        public UsersController(ApplicationDbContext context ,UserManager<User> userManager,RoleManager<IdentityRole> roleManager, SignInManager<User> signInManager, ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _context = context;       
            _signInManager = signInManager;
        }

        public IActionResult Profil()
        {
            return View();
        }
      
        [Authorize(Roles = "Admin, Manager")]
        public IActionResult DefaultPermission() 
        {
            return View();
        }

        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> UserList()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var currentRole = await _userManager.GetRolesAsync(currentUser);
            bool isManager = currentRole.Contains("Manager");
            ViewBag.IsManager = isManager;
            var users = _userManager.Users.ToList();

            var isAdmin = _userManager.IsInRoleAsync(currentUser, "Admin").Result;

            List<UserViewModel> usersViewModel;

            if (isAdmin)
            {
                usersViewModel = users
                    .Select(u => new
                    {
                        User = u,
                        Role = _userManager.GetRolesAsync(u).Result.FirstOrDefault()
                    })
                    .Select(x => new UserViewModel
                    {
                        Id = x.User.Id,
                        FullName = x.User.FullName,
                        Email = x.User.Email,
                        PhoneNumber = x.User.PhoneNumber,
                        Company = x.User.Company,
                        Role = x.Role,
                        EmailConfirmed = x.User.EmailConfirmed,
                        IsAccessRestricted = x.User.LockoutEnd != null,
                        AccessEndTime = x.User.LockoutEnd.HasValue ? x.User.LockoutEnd.Value.UtcDateTime.ToString("dd/MM/yyyy HH:mm:ss") : null,
                    })
                    .ToList();
            }
            else if (isManager)
            {
                usersViewModel = users
                    .Select(u => new
                    {
                        User = u,
                        Role = _userManager.GetRolesAsync(u).Result.FirstOrDefault()
                    })
                    .Where(x => x.Role == "User" || x.Role == "Manager")
                    .Select(x => new UserViewModel
                    {
                        Id = x.User.Id,
                        FullName = x.User.FullName,
                        Email = x.User.Email,
                        PhoneNumber = x.User.PhoneNumber,
                        Company = x.User.Company,
                        Role = x.Role,
                        EmailConfirmed = x.User.EmailConfirmed,
                        IsAccessRestricted = x.User.LockoutEnd != null,
                        AccessEndTime = x.User.LockoutEnd.HasValue ? x.User.LockoutEnd.Value.UtcDateTime.ToString("dd/MM/yyyy HH:mm:ss") : null,

                    })
                    .ToList();
            }

            else
            {
                usersViewModel = new List<UserViewModel>();
            }

            var roles = _roleManager.Roles.ToList();

            var options = new List<SelectListItem>();
            foreach (var role in roles)
            {
                options.Add(new SelectListItem { Value = role.Name, Text = role.Name });
            }

            var classNames = GetClassNamesFromNamespace("RPRM.Models.Metiers");

            var userListViewModel = new UserListViewModel
            {
                Users = usersViewModel,
                ClassNames = classNames
            };

            ViewData["RoleOptions"] = options;
            return View(userListViewModel);
        }
        [Authorize(Roles = "Admin, Manager")]
        [HttpPost]
        public async Task<IActionResult> ConfirmEmail(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.EmailConfirmed = true;
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return Ok();
                }
                return BadRequest(result.Errors);
            }
            return BadRequest("User not found");
        }
        [Authorize(Roles = "Admin, Manager")]
        [HttpPost]
        public async Task<IActionResult> UpdateAccessRestriction(string userId, string restrictionType, int? restrictionAmount)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Contains("Admin") || userRoles.Contains("Manager"))
            {
                // Si l'utilisateur est un administrateur ou un manager, ne faites rien
                return RedirectToAction(nameof(UserList));
            }
            if (restrictionType == null || restrictionAmount == null)
            {
                user.LockoutEnd = null;
            }
            else
            {
                var restrictionEnd = DateTimeOffset.Now;

                switch (restrictionType)
                {
                    case "Hour":
                        restrictionEnd = restrictionEnd.AddHours(restrictionAmount.Value);
                        break;
                    case "Day":
                        restrictionEnd = restrictionEnd.AddDays(restrictionAmount.Value);
                        break;
                    case "Month":
                        restrictionEnd = restrictionEnd.AddMonths(restrictionAmount.Value);
                        break;
                    case "Year":
                        restrictionEnd = restrictionEnd.AddYears(restrictionAmount.Value);
                        break;
                }

                user.LockoutEnd = restrictionEnd;
            }

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                // Handle the error
            }

            return RedirectToAction(nameof(UserList));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateUser(string fullName, string company, string role, string phoneNumber)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Impossible de charger l'utilisateur avec l'ID '{_userManager.GetUserId(User)}'.");
            }

            user.FullName = fullName;
            user.Company = company;
            //user.Role = role;
            user.PhoneNumber = phoneNumber;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View("Profil", user);
        }
  
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateUserByAdmin(UpdateUserAdmin model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);

            if (user == null)
            {
                return NotFound();
            }

            user.FullName = model.FullName;
            user.Company = model.Company;
            user.PhoneNumber = model.PhoneNumber;

            var newRole =model.Role;
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            var userPermission = _context.UserPermissions.FirstOrDefault(up => up.UserId == user.Id);
            if (userPermission != null)
            {
                _context.UserPermissions.Remove(userPermission);
                await _context.SaveChangesAsync();
            }
            await _userManager.UpdateAsync(user);

            return RedirectToAction("UserList", "Users");
        }
        [HttpGet]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var model = new UpdateUserAdmin
            {
                FullName = user.FullName,
                Company = user.Company,
                PhoneNumber = user.PhoneNumber,
                Role = (await _userManager.GetRolesAsync(user))[0]
            };
            return Json(model);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteSelected(string[] selectedIds)
        {
            foreach (var id in selectedIds)
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user == null)
                {
                    return NotFound();
                }

                await _userManager.DeleteAsync(user);
            }

            return RedirectToAction("UserList", "Users");
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdatePerm(string userId, List<Permission> permissions)
        {
            // Récupérez l'utilisateur en fonction de l'ID de l'utilisateur
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Mettez à jour les permissions pour cet utilisateur en fonction des informations reçues
            var existingPermission = _context.UserPermissions.FirstOrDefault(p => p.UserId == user.Id);

            if (existingPermission != null)
            {
                _context.UserPermissions.Remove(existingPermission);
                await _context.SaveChangesAsync();
            }
            // Filtrer les permissions avec au moins CanView ou CanEdit sélectionné
            var filteredPermissions = permissions.Where(p => p.CanView || p.CanEdit).ToList();

            // Convertissez les permissions filtrées en JSON
            var permissionsJson = JsonSerializer.Serialize(filteredPermissions);

            // Ajoutez les nouvelles permissions pour cet utilisateur
            var userPermission = new UserPermission
            {
                UserId = user.Id,
                Permissions = permissionsJson
            };

            _context.UserPermissions.Add(userPermission);
            await _context.SaveChangesAsync();
            return RedirectToAction("UserList");
        }
        [HttpGet]
        public async Task<IActionResult> GetUserPermissions(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var existingPermission = _context.UserPermissions.FirstOrDefault(p => p.UserId == user.Id);

            if (existingPermission != null)
            {
                var permissions = JsonSerializer.Deserialize<List<Permission>>(existingPermission.Permissions);
                return Json(permissions);
            }

            return Json(new List<Permission>());
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                return new JsonResult(new { Message = "Les données du formulaire ne sont pas valides.", Type = "error" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return new JsonResult(new { Message = "Impossible de charger l'utilisateur.", Type = "error" });
            }

            if (newPassword != confirmPassword)
            {
                return new JsonResult(new { Message = "Le nouveau mot de passe et le mot de passe de confirmation ne correspondent pas.", Type = "error" });
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            if (!changePasswordResult.Succeeded)
            {
                var errors = changePasswordResult.Errors.Select(x => x.Description).ToList();
                return new JsonResult(new { Message = "Erreur lors du changement de mot de passe. Veuillez réessayer.", Errors = errors, Type = "error" });
            }

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("L'utilisateur a changé son mot de passe avec succès.");
            return new JsonResult(new { Message = "Votre mot de passe a été modifié avec succès.", Type = "success" });
        }

        public List<string> GetClassNamesFromNamespace(string namespaceFilter)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var classNames = new List<string>();

            foreach (Type type in assembly.GetTypes())
            {
                if (type.Namespace == namespaceFilter && type.IsClass)
                {
                    classNames.Add(type.Name);
                }
            }

            return classNames;
        }

    }
}

