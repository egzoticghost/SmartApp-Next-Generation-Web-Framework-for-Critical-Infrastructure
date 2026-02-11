using AspnetUserApi;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SmartWebApp.Data; // Ensure the namespace 'SmartWebApp.Data' exists in your project
using SmartWebApp.Services; // Add this using directive if AuthService is in this namespace
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://smartwebapi.local")
});

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SmartWebApp.Data.UserDbContext>(options =>
    options.UseNpgsql(connectionString));

// Autofac
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(container =>
{
    container.RegisterType<UserRepository>().As<IUserRepository>().InstancePerLifetimeScope();
    container.RegisterType<UserService>().As<IUserService>().InstancePerLifetimeScope();
});

// Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect("oidc", options =>
{
    var config = builder.Configuration.GetSection("Authentication:Keycloak");
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.Authority = config["Authority"].Trim();
    options.ClientId = config["ClientId"];
    options.ClientSecret = config["ClientSecret"];
    options.CallbackPath = config["CallbackPath"];
    options.ResponseType = config["ResponseType"];
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
});

// JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Adres URL Twojego Keycloaka (widoczny w klastrze)
        options.Authority = "http://keycloak.local/realms/SmartAppRealm";
        options.Audience = "smart-api";
        options.RequireHttpsMetadata = false; // Bo lokalnie używamy HTTP

        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true
        };
    });

// MVC / Swagger
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorPolicy", policy =>
    {
        policy.WithOrigins("http://smartapp.local") // Adres Twojego frontendu
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// HTTP pipeline
// if (app.Environment.IsDevelopment())
// {
    // app.UseSwagger();
    // app.UseSwaggerUI();
// }

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("BlazorPolicy");
app.MapControllers();
app.MapDefaultControllerRoute();

app.Run();

[Authorize]
public class HomeController : Controller
{
    [Authorize]
    public IActionResult Secure()
    {
        return View(); // widok dostępny tylko po zalogowaniu
    }

    public IActionResult Login()
    {
        return Challenge(new AuthenticationProperties { RedirectUri = "/secure" }, "oidc");
    }

    public IActionResult Logout()
    {
        return SignOut(new AuthenticationProperties { RedirectUri = "/" }, "Cookies", "oidc");
    }
}
