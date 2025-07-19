using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserRoles.Models;
using UserRoles.ViewModels;
using UserRoles.Services; // Added to recognize IEmailService

namespace UserRoles.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Users> signInManager;
        private readonly UserManager<Users> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IEmailService emailService; //  Added this line to declare email service

        // Injecting identity services into this controller
        public AccountController(SignInManager<Users> signInManager, UserManager<Users> userManager, RoleManager<IdentityRole> roleManager, IEmailService emailService) // Injected email service
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.emailService = emailService; // Assigned injected email service
        }

        // Load the login form
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
                return View(model);

            // Find user by email to check if email is confirmed and active
            var user = await userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                if (!await userManager.IsEmailConfirmedAsync(user))
                {
                    ModelState.AddModelError(string.Empty, "Email is not confirmed.");
                    return View(model);
                }

                if (!user.IsActive)
                {
                    ModelState.AddModelError(string.Empty, "Your account is not approved yet.");
                    return View(model);
                }
            }

            var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Tickets");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        // Just showing a message while waiting for admin to approve registration
        [HttpGet]
        public IActionResult PendingApproval()
        {
            return View();
        }

        // Load registration form
        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.Roles = new List<string> { "Developer", "Tester", "Project Leader" };
            return View();
        }

        // Handle registration submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            ViewBag.Roles = new List<string> { "Developer", "Tester", "Project Leader" };

            if (!ModelState.IsValid)
                return View(model);

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
                // Create role if it doesn't exist
                var roleExists = await roleManager.RoleExistsAsync(model.Role);
                if (!roleExists)
                {
                    await roleManager.CreateAsync(new IdentityRole(model.Role));
                }

                // Add user to role
                await userManager.AddToRoleAsync(user, model.Role);

                // Send welcome email (no confirmation token/link required)
                try
                {
                    await emailService.SendEmailAsync(
    user.Email,
    "Welcome to OmniTrack!",
    $@"
    <div style='text-align:center;'>
        <img src='https://i.imgur.com/J9Z3ce5.jpeg' alt='OmniTrack Logo' style='max-width:200px; margin-bottom:20px;'/>
    </div>
    <p>Welcome {user.FullName},</p>
    <p>Thank you for registering with <strong>OmniTrack</strong>.</p>
    <p>You have been registered successfully as a <strong>{model.Role}</strong>.</p>
    <p>You will be notified once your account is approved.</p>
    <p>Kind regards,<br/>OmniTrack Team</p>");

                }
                catch (Exception ex)
                {
                    TempData["Message"] = "User registered but welcome email failed to send.";
                }

                // Redirect user to Pending Approval screen
                return RedirectToAction("PendingApproval", "Account");
            }

            // If we get here, registration failed – show errors
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }


        // Show a 403 screen if a user isn't allowed to do something
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Step 1: Check if the user email exists
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
                return View(model);

            var user = await userManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found!");
                return View(model);
            }

            return RedirectToAction("ChangePassword", "Account", new { username = user.UserName });
        }

        // Step 2: Let user change password
        [HttpGet]
        public IActionResult ChangePassword(string username)
        {
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("VerifyEmail", "Account");

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
                return RedirectToAction("Login", "Account");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        // Ends user session
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

    }

    }

