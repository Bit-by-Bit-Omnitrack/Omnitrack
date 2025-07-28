using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using UserRoles.Data;
using UserRoles.Models;
using UserRoles.Services; // for email + seeding
using Microsoft.AspNetCore.Authentication.JwtBearer; // Required for JWT Bearer
using Microsoft.IdentityModel.Tokens; // Required for SecurityKey and TokenValidationParameters
using System.Text; // Required for Encoding.UTF8


var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel();

// This is for adding support for MVC + API controllers
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// --- JWT Configuration START ---
// You should store these values securely, e.g., in appsettings.json or environment variables
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var jwtKey = builder.Configuration["Jwt:Key"]; // This should be a strong, secret key

// Add Authentication services
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});
// --- JWT Configuration END ---

// Swagger configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "OmniTrack", Version = "v1" });

    // --- Swagger JWT Security Definition START ---
    c.AddSecurityDefinition("OmniTrack", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: OmniTrack {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "OmniTrack"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "OmniTrack"
                },
                Scheme = "oauth2",
                Name = "OmniTrack",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
    // --- Swagger JWT Security Definition END ---

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});


// Add our EmailService so we can inject and send real emails
builder.Services.AddScoped<IEmailService, EmailService>(); // use scoped for DB-safe access
builder.Services.AddScoped<SeedService>(); // Needed to make seeding work when called


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

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

// Configure Application Cookie (still relevant for MVC views if you use them)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login"; // explicitly set login path
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

// ?? Auto-create database on first run
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<AppDbContext>();
     dbContext.Database.Migrate(); // Applies migrations
    await SeedService.SeedDatabase(services); // Run seeding inside same scope
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Good for development
    app.UseSwagger(); // Enable Swagger middleware
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OmniTrack V1");

        // c.RoutePrefix = string.Empty;
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // added static files middleware

app.UseRouting();

// --- IMPORTANT: UseAuthentication and UseAuthorization must be after UseRouting and before MapControllerRoute ---
app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");


app.Run();

