using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserRoles.Data;
using UserRoles.Models;
using UserRoles.Services;

var builder = WebApplication.CreateBuilder(args);

// Add support for MVC views and controllers
builder.Services.AddControllersWithViews();

// Configure EF Core to use SQL Server with connection string from appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Configure Identity with the custom Users model
builder.Services.AddIdentity<Users, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Redirect users to AccessDenied if they try accessing unauthorized resources
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

// Automatically apply any pending EF Core migrations when app starts
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate(); // Runs migrations if needed
}

// Seed roles and the admin user into the database (this runs only once)
await SeedService.SeedDatabase(app.Services);

/*
    This section was just used to reset my admin password the first time.
    Now that I’ve logged in successfully, I’m commenting it out so it doesn’t keep resetting the password every time I run the project.
*/


/*
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Users>>();

    var adminEmail = "millicent9710@gmail.com"; // My admin email used in SQL
    var newPassword = "SecureAdmin@2025!";      // A strong password I used for first login

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser != null)
    {
        var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
        var result = await userManager.ResetPasswordAsync(adminUser, token, newPassword);

        if (result.Succeeded)
        {
            Console.WriteLine("Admin password reset successfully.");
        }
        else
        {
            foreach (var error in result.Errors)
            {
                Console.WriteLine(error.Description);
            }
        }
    }
    else
    {
        Console.WriteLine("Could not find admin user with email: " + adminEmail);
    }
}
*/

// Handle global errors in production
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middleware pipeline
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// Set the default route to go to Account/Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();
