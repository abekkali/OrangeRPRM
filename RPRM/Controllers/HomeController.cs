using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RPRM.Data;
using RPRM.Models;
using RPRM.Models.User;
using System.Diagnostics;

namespace RPRM.Controllers
{
    // Autorisation requise pour accéder aux actions  accessible pour User,Manager,Admin
    [Authorize]
    public class HomeController : Controller
    {
        // Gestionnaire d'utilisateurs pour les opérations liées aux utilisateurs et le deuxième connexion pour les opérations d'authentification 
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        public HomeController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public async Task<IActionResult> Index()
        {
            // Vérifie si l'utilisateur est connecté
            if (_signInManager.IsSignedIn(User))
            {
                var user = await _userManager.GetUserAsync(User);
            }

            // Renvoie la vue Index
            return View();
        }

        // Action PageInformation qui prend en paramètre un message et ne nécessite pas d'authentification
        [AllowAnonymous]
        public IActionResult PageInformation(string message)
        {
            // Stocke le message dans ViewData pour l'afficher dans la vue
            ViewData["Message"] = message;
            // Renvoie la vue PageInformation
            return View();
        }

        // Action Error pour afficher la page d'erreur
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Crée un nouvel objet ErrorViewModel et définit l'identifiant de la requête
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
