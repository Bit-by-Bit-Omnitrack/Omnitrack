using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserRoles.Data;
using UserRoles.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace UserRoles.Services
{
    public class SeedService
    {
        public static async Task SeedDatabase(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Users>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedService>>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            try
            {
                logger.LogInformation("Ensuring the database is created.");
                await context.Database.EnsureCreatedAsync();

                logger.LogInformation("Seeding roles.");
                await AddRoleAsync(roleManager, "System Administrator");
                await AddRoleAsync(roleManager, "User");

                var admins = new List<(string Email, string FullName)>
                {
                    ("millicent9710@gmail.com", "Millicent Mogane"),
                    ("tlotlomolefem@gmail.com", "Tlotlo Molefe"),
                    ("emkhabela2@gmail.com", "Esther Mkhabela"),
                    ("jokweniazola@gmail.com", "Azola Jokweni")
                };

                foreach (var (email, fullName) in admins)
                {
                    logger.LogInformation($"Processing admin user: {email}");
                    var user = await userManager.FindByEmailAsync(email);

                    if (user == null)
                    {
                        user = new Users
                        {
                            FullName = fullName,
                            UserName = email,
                            NormalizedUserName = email.ToUpper(),
                            Email = email,
                            NormalizedEmail = email.ToUpper(),
                            EmailConfirmed = true,
                            SecurityStamp = Guid.NewGuid().ToString(),
                            IsActive = true
                        };

                        var result = await userManager.CreateAsync(user, "SecureAdmin@2025");
                        if (result.Succeeded)
                        {
                            logger.LogInformation($"Created new admin: {email}");
                        }
                        else
                        {
                            logger.LogError("Failed to create admin user {Email}: {Errors}", email, string.Join(", ", result.Errors.Select(e => e.Description)));
                            continue; // skip to next admin
                        }
                    }
                    else
                    {
                        // Update existing user to confirm and activate
                        user.FullName = fullName;
                        user.EmailConfirmed = true;
                        user.IsActive = true;
                        await userManager.UpdateAsync(user);
                        logger.LogInformation($"Updated existing user {email}: confirmed and activated.");
                    }

                    if (!await userManager.IsInRoleAsync(user, "System Administrator"))
                    {
                        await userManager.AddToRoleAsync(user, "System Administrator");
                        logger.LogInformation($"Assigned 'System Administrator' role to: {email}");
                    }

                    // Send welcome email
                    var subject = "Welcome to OmniTrack!";
                    var body = $@"
                        <p>Welcome {fullName},</p>
                        <p>Thank you for registering with <strong>OmniTrack</strong>.</p>
                        <p>You have been registered successfully as a System Administrator.</p>
                        <p>Kind regards,<br/>OmniTrack Team</p>";

                    await emailService.SendEmailAsync(email, subject, body);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        private static async Task AddRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
