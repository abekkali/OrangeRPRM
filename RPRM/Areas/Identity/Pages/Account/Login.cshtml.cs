// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using RPRM.Models.User;

namespace RPRM.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<User> signInManager, ILogger<LoginModel> logger, UserManager<User> userManager)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null)
                {
                    _logger.LogInformation("L'utilisateur avec l'e-mail {Email} n'a pas été trouvé.", Input.Email);
                    ModelState.AddModelError(string.Empty, "La tentative de connexion n'est pas valide.");
                    return Page();
                }

                if (!user.EmailConfirmed)
                {
                    _logger.LogInformation("L'utilisateur avec l'e-mail {Email} n'a pas confirmé son e-mail.", Input.Email);
                    ModelState.AddModelError(string.Empty, "Veuillez confirmer votre adresse e-mail avant de vous connecter.");
                    return Page();
                }
                var passwordHasher = new PasswordHasher<IdentityUser>();
                var providedPassword = "4ftns9ieV:JGiGx";
                var hashedPassword = "AQAAAAIAAYagAAAAED6Zm4VGOaT/hmwjBfIlqTM3N+wh7+VBhgG9MiWwaiPoLB0rvh3miGEbd/dYYphLRw==";

                var verificationResult = passwordHasher.VerifyHashedPassword(new IdentityUser(), hashedPassword, providedPassword);

                if (verificationResult == PasswordVerificationResult.Success)
                {
                    Console.WriteLine("Le mot de passe est correct.");
                }
                else
                {
                    Console.WriteLine("Le mot de passe n'est pas correct.");
                }

                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    _logger.LogInformation("L'utilisateur s'est connecté.");
                    return LocalRedirect(returnUrl);
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Le compte de l'utilisateur est verrouillé.");
                    var message = "Votre compte est verrouillé";
                    return RedirectToAction("PageInformation", "Home", new { message });
                }

                if (result.IsNotAllowed)
                {
                    _logger.LogWarning("La connexion pour l'utilisateur avec l'e-mail {Email} n'est pas autorisée.", Input.Email);
                    ModelState.AddModelError(string.Empty, "La connexion n'est pas autorisée.");
                    return Page();
                }

                if (result.RequiresTwoFactor)
                {
                    _logger.LogWarning("La connexion pour l'utilisateur avec l'e-mail {Email} nécessite une authentification à deux facteurs.", Input.Email);
                    ModelState.AddModelError(string.Empty, "Une authentification à deux facteurs est requise.");
                    return Page();
                }

                // Si aucun des cas ci-dessus n'est vrai, cela signifie que la connexion a échoué pour une autre raison
                _logger.LogWarning("La tentative de connexion pour l'utilisateur avec l'e-mail {Email} a échoué.", Input.Email);
                ModelState.AddModelError(string.Empty, "La tentative de connexion n'est pas valide.");
                return Page();

            }

            return Page();
        }
    }
}
