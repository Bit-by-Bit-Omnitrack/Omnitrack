using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserRoles.Models;
using UserRoles.Services; // Added to use our custom IEmailService for sending emails
using UserRoles.ViewModels;
using UserRoles.Data; // Added to allow logging email activity to the database

namespace UserRoles.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Users> signInManager;
        private readonly UserManager<Users> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        
        //  Injecting a custom email service to send confirmation emails after registration
        private readonly IEmailService emailService; // Added for email service
        private readonly AppDbContext _context; // Used to log email results to the EmailLogs table


        public AccountController(
        SignInManager<Users> signInManager,
        UserManager<Users> userManager,
        RoleManager<IdentityRole> roleManager,
        IEmailService emailService,// Added this to inject email functionality
         AppDbContext context      // Injected AppDbContext for email logging

        )
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.emailService = emailService;
            this._context = context; // Assigned the injected context to the local field

        }



        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                if (!user.IsActive)
                {
                    ModelState.AddModelError(string.Empty, "Your account is not active. Please wait for approval.");
                    return RedirectToAction("AwaitingApproval", "Account"); // Redirect to awaiting screen;
                }
            }

            var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Tickets1");
            }

            ModelState.AddModelError(string.Empty, "Invalid Login Attempt.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.Roles = new List<string> { "Developer", "Tester", "Project Leader" }; // Removed "System Administrator"
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            ViewBag.Roles = new List<string> { "Developer", "Tester", "Project Leader" }; // Removed "System Administrator"

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new Users
            {
                FullName = model.Name,
                UserName = model.Email,
                Email = model.Email,
                NormalizedUserName = model.Email.ToUpper(),
                NormalizedEmail = model.Email.ToUpper(),
                IsActive = false // User cannot log in until approved
            };

            var result = await userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var roleExist = await roleManager.RoleExistsAsync(model.Role);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(model.Role));
                }



                // Send email and log result in EmailLogs table
                try
                {
                    await emailService.SendEmailAsync(
                        model.Email,
                        "OmniTrack Registration Received",
                        $"Dear {model.Name},\n\nThank you for registering with OmniTrack. Your account has been successfully created.\n\nPlease note that your access is currently pending approval by a System Administrator. You will be notified once your account is activated.\n\nIf you believe this is a mistake or have any questions, feel free to contact support.\n\nBest regards,\nThe OmniTrack Team"
                    );

                    // Log success
                    var log = new EmailLog
                    {
                        Recipient = model.Email,
                        Subject = "OmniTrack Registration Received",
                        SentAt = DateTime.UtcNow,
                        IsSuccess = true
                    };
                    _context.EmailLogs.Add(log);
                }
                catch (Exception ex)
                {
                    // Log failure
                    var log = new EmailLog
                    {
                        Recipient = model.Email,
                        Subject = "OmniTrack Registration Received",
                        SentAt = DateTime.UtcNow,
                        IsSuccess = false,
                        ErrorMessage = ex.Message
                    };
                    _context.EmailLogs.Add(log);
                }

                //  Save the log entry
                await _context.SaveChangesAsync();


                await userManager.AddToRoleAsync(user, model.Role);

                // await signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("AwaitingApproval", "Account");

            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
        // In AccountController.cs
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }


        [HttpGet]
        public IActionResult VerifyEmail()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await userManager.FindByNameAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "User not found!");
                return View(model);
            }
            else
            {
                return RedirectToAction("ChangePassword", "Account", new { username = user.UserName });
            }
        }

        [HttpGet]
        public IActionResult ChangePassword(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("VerifyEmail", "Account");
            }

            return View(new ChangePasswordViewModel { Email = username });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Something went wrong");
                return View(model);
            }

            var user = await userManager.FindByNameAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "User not found!");
                return View(model);
            }

            var result = await userManager.RemovePasswordAsync(user);
            if (result.Succeeded)
            {
                result = await userManager.AddPasswordAsync(user, model.NewPassword);
                TempData["Message"] = "Your password has been successfully updated. You may now log in once your account is approved.";
                return RedirectToAction("AwaitingApproval", "Account");

            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult AwaitingApproval() 
        {
            return View();
        }
    }
}
