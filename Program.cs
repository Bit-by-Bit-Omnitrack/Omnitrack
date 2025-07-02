using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserRoles.Data;
using UserRoles.Models;
using UserRoles.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container (MVC controllers with views)
builder.Services.AddControllersWithViews();

// Configure the database context with SQL Server connection string from appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Configure Identity with password and user settings
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

// Register EmailService for dependency injection so it can be injected where needed
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

// Automatically apply any pending migrations and create the database if it doesn't exist
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

// Seed the database with default data such as roles and admin user
await SeedService.SeedDatabase(app.Services);

// Configure middleware for handling requests and responses
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");  // Use error page in production
    app.UseHsts();                           // Use HTTP Strict Transport Security
}

app.UseHttpsRedirection();  // Redirect HTTP requests to HTTPS
app.UseStaticFiles();       // Serve static files like CSS, JS, images

app.UseRouting();           // Enable routing

app.UseAuthentication();    // Enable authentication middleware
app.UseAuthorization();     // Enable authorization middleware

// Map default controller route to Account/Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();  // Run the application
