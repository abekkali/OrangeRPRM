using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using RPRM.Models.User;

public class RegisterModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<RegisterModel> _logger;
    private readonly IEmailSender _emailSender;

    public RegisterModel(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ILogger<RegisterModel> logger,
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _emailSender = emailSender;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public string ReturnUrl { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Le {0} doit contenir au moins {2} caractères et au maximum {1} caractères.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmer le mot de passe")]
        [Compare("Password", ErrorMessage = "Le mot de passe et la confirmation du mot de passe ne correspondent pas.")]
        public string ConfirmPassword { get; set; }
    }

    public async Task OnGetAsync(string returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        // Si returnUrl est null, définir returnUrl sur la page d'accueil
        returnUrl ??= Url.Content("~/");
        // Vérifier si le modèle est valide
        if (ModelState.IsValid)
        {
            // Vérifier si l'adresse e-mail fait partie du domaine orange
            //if (Input.Email.EndsWith("@orange.com"))
            if(true)
            {
                // Créer un nouvel utilisateur avec l'e-mail fourni
                var user = new User { UserName = Input.Email, Email = Input.Email };
                // Créer l'utilisateur avec le mot de passe fourni
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    // Assigner le rôle par défaut à l'utilisateur
                    var addToRoleResult = await _userManager.AddToRoleAsync(user, "User");
                    
                    // Vérifier si un compte confirmé est requis pour se connecter
                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email });
                    }
                    else
                    {
                        // Connecter l'utilisateur
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                // Ajouter les erreurs au modèle, si nécessaire
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Seules les adresses e-mail @orange.com sont autorisées.");
                return Page();
            }
        }
        // Si nous sommes arrivés jusqu'ici, quelque chose a échoué, réafficher le formulaire
        return Page();
    }
}
