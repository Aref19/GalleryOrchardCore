using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Users;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Gallery.Module.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IUser> _signInManager;
        private readonly UserManager<IUser> _userManager;

        public AccountController(
            SignInManager<IUser> signInManager,
            UserManager<IUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // ViewModels direkt im Controller definiert
        public class LoginViewModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public class RegisterViewModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
    
            if (ModelState.IsValid)
            {
                // Zuerst den Benutzer anhand der E-Mail finden
                var user = await _userManager.FindByEmailAsync(model.Email);
        
                if (user != null)
                {
                    // Dann mit dem Benutzernamen anmelden
                    var result = await _signInManager.PasswordSignInAsync(
                        user.UserName, 
                        model.Password, 
                        model.RememberMe, 
                        lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        return RedirectToLocal(returnUrl);
                    }
                }
        
                // Bei Misserfolg einen Fehler anzeigen
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            return View(model);
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                
                string username = model.Email.Split('@')[0]; 
              
                username = new string(username.Where(c => char.IsLetterOrDigit(c)).ToArray());

                var user = new OrchardCore.Users.Models.User { UserName = username, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToLocal(returnUrl);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }
    

        private IActionResult RedirectToLocal(string returnUrl)
        {
            return Redirect("/");
        }
    }
}