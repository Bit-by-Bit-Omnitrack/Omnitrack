using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Omnitrack.Models.ViewModels; // Add this using directive
using System.Security.Policy;

namespace Omnitrack.Controllers
{
    public class AccountRegistrationController : Controller
    {
        public class AccountController : Controller
        {
            private readonly UserManager<IdentityUser> _userManager;
            private readonly SignInManager<IdentityUser> _signInManager;
            private readonly IEmailSender<IdentityUser> _emailSender; // Fixed: Added the required type argument

            public AccountController(UserManager<IdentityUser> userManager,
                                     SignInManager<IdentityUser> signInManager,
                                     IEmailSender<IdentityUser> emailSender) // Fixed: Updated constructor parameter
            {
                _userManager = userManager;
                _signInManager = signInManager;
                _emailSender = emailSender;
            }

            [HttpGet]
            public IActionResult Register() => View();

            [HttpPost]
            public async Task<IActionResult> Register(  RegisterViewModel model)
            {
                if (!ModelState.IsValid) return View(model);

                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmationLink = Url.Action("ConfirmEmail", "Account",
                        new { userId = user.Id, token }, Request.Scheme);

                    await _emailSender.SendConfirmationLinkAsync(user, model.Email, confirmationLink); // Updated method call

                    return RedirectToAction("Login");
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(model);
            }

            public async Task<IActionResult> ConfirmEmail(string userId, string token)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return NotFound();

                var result = await _userManager.ConfirmEmailAsync(user, token);
                return View(result.Succeeded ? "ConfirmEmail" : "Error");
            }
        }
    }
}
