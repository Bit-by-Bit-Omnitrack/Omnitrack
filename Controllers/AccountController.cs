using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserRoles.Models;
using UserRoles.ViewModels;
using UserRoles.Services;
using UserRoles.Data;
using Microsoft.EntityFrameworkCore;



namespace UserRoles.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Users> signInManager;
        private readonly UserManager<Users> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IEmailService emailService;
        private readonly AppDbContext _context;



        public AccountController(SignInManager<Users> signInManager, UserManager<Users> userManager, RoleManager<IdentityRole> roleManager, IEmailService emailService, AppDbContext context)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.emailService = emailService;
            this._context = context;
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
                return View(model);

            var user = await userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Your account is not approved yet.");
                return View(model);
            }

            var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);


            if (result.Succeeded)
            {
                // ADDED: Get user roles
                var roles = await userManager.GetRolesAsync(user);

                // ADDED: Redirect based on role
                if (roles.Contains("System Administrator")) // or "Admin" depending on your role name
                    return RedirectToAction("AdminDashboard", "Dashboard"); 

                if (roles.Contains("Project Leader"))
                    return RedirectToAction("LeaderDashboard", "Dashboard"); 

                if (roles.Contains("Developer"))
                    return RedirectToAction("DeveloperDashboard", "Dashboard"); 

                if (roles.Contains("Tester"))
                    return RedirectToAction("TesterDashboard", "Dashboard"); 

                // Fallback redirect
                return RedirectToAction("Index", "Home"); 
            }
            else if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Account locked due to multiple failed attempts.");
            }
            else if (result.IsNotAllowed)
            {
                ModelState.AddModelError(string.Empty, "Login is not allowed for this user.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult PendingApproval()
        {
            ViewData["UserName"] = TempData["UserName"] ?? "User";
            return View();
        }



        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.Roles = new List<string> { "Developer", "Tester", "Project Leader" };
            return View();
        }

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

                IsActive = false,

            };

            var result = await userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var isSystemAdmin = await _context.SystemAdmins
                    .AnyAsync(sa => sa.Email.ToLower() == model.Email.ToLower());

                string roleToAssign = isSystemAdmin ? "System Administrator" : model.Role;

                IdentityResult roleResult;

                if (isSystemAdmin)
                {
                    user.IsActive = true; // Automatically activate System Admins

                    // Ensure role exists
                    if (!await roleManager.RoleExistsAsync("System Administrator"))
                    {
                        await roleManager.CreateAsync(new IdentityRole("System Administrator"));
                    }

                    // Assign role
                    roleResult = await userManager.AddToRoleAsync(user, "System Administrator");

                    // Save updated IsActive value
                    await userManager.UpdateAsync(user);
                }
                else
                {
                    // For regular users, ensure selected role exists
                    if (!await roleManager.RoleExistsAsync(model.Role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(model.Role));
                    }

                    // Assign role
                    roleResult = await userManager.AddToRoleAsync(user, model.Role);
                }

                // Check if role assignment succeeded
                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }


                if (roleToAssign == "System Administrator")
                {
                    Console.WriteLine($"{user.Email} registered as System Administrator.");
                    user.IsActive = true;

                    var updateResult = await userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        foreach (var error in updateResult.Errors)
                        {
                            ModelState.AddModelError("", $"Failed to auto-approve system admin: {error.Description}");
                        }
                        return View(model);
                    }
                }

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
<p>You have been registered successfully as a <strong>{roleToAssign}</strong>.</p>
<p>You will be notified once your account is approved.</p>
<p>Kind regards,<br/>OmniTrack Team</p>"
                    );
                }
                catch (Exception ex)
                {
                    TempData["Message"] = "User registered but welcome email failed to send.";
                    Console.WriteLine(ex.Message);
                }

                TempData["UserName"] = user.FullName;
                TempData["Message"] = $"{user.FullName}, your registration was successful. Awaiting approval.";
                return RedirectToAction("PendingApproval", "Account");


            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveUser(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = true;
            await userManager.UpdateAsync(user);

            await emailService.SendEmailAsync(
                user.Email,
                "Account Approved",
                $@"<p>Hello {user.FullName},</p>
           <p>Your account on <strong>OmniTrack</strong> has been approved.</p>
           <p>You can now log in and access the system.</p>
           <p>Regards,<br/>OmniTrack Team</p>"
            );

            TempData["Message"] = $"User {user.FullName} approved successfully.";
            return RedirectToAction("Authenticate", "User"); //  go back to admin approval list
        }


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
                return View(model);

            var user = await userManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found!");
                return View(model);
            }

            return RedirectToAction("ChangePassword", "Account", new { username = user.UserName });
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }


    }
}
