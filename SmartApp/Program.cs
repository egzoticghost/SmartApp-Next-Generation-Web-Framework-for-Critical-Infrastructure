using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication; // Ensure the package is installed
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
//using Microsoft.AspNetCore.Authentication.JwtBearer; // Add this using directive
using Microsoft.Extensions.DependencyInjection; // Add this using directive
//using Microsoft.Extensions.Http; // Add this using directive
using Microsoft.Extensions.DependencyInjection.Extensions; // Add this using directive
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop;
using Radzen;
//using SmartApp.Services; // Add this using directive
//using SmartApp.Shared; // Add this using directive
using Radzen.Blazor; // Add this using directive for Radzen components
using SmartApp;
using SmartApp.Handlers; // Add this using directive
//using Radzen.Blazor.Services; // Add this using directive for Radzen services
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using SmartApp.Data; // Add this using directive for AppDbContext
using Microsoft.EntityFrameworkCore; // Add this using directive

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://smartapp.local")
});

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Ensure the AddHttpClient extension method is available
//builder.Services.AddHttpClient("API", client =>
//{
//    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
//})
//.AddHttpMessageHandler<TokenValidationHandler>();

//builder.Services.AddScoped(sp =>
//    sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

// 1. Register your TokenValidationHandler from the SmartApp.Handlers namespace
builder.Services.AddScoped<TokenValidationHandler>();

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7325") // Backend WebAPI
});

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://server") // odwo³anie do backendu z docker-compose
});

builder.Services.AddTransient<TokenValidationHandler>();
//builder.Services.AddHttpClient("SecureClient")
//    .AddHttpMessageHandler<TokenValidationHandler>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))); // np. PostgreSQL

// 2. Configure a named HttpClient that uses the TokenValidationHandler
//builder.Services.AddHttpClient("API", client =>
//{
//    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
//})
//.AddHttpMessageHandler<TokenValidationHandler>();

// 3. Optionally, make this named client the default one
//builder.Services.AddScoped(sp =>
//    sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

// Add OpenID Connect authentication
//builder.Services.AddOidcAuthentication(options =>
//{
//    builder.Configuration.Bind("Oidc", options.ProviderOptions);
//    // Example manual config:
//    // options.ProviderOptions.Authority = "https://your-openid-provider.com";
//    // options.ProviderOptions.ClientId = "your-client-id";
//    // options.ProviderOptions.ResponseType = "code";
//    // options.ProviderOptions.DefaultScopes.Add("openid");
//    // options.ProviderOptions.DefaultScopes.Add("profile");
//    // options.ProviderOptions.DefaultScopes.Add("email");
//});

//builder.Services.AddAuthentication("Bearer")
//    .AddJwtBearer("Bearer", options =>
//    {
//        options.Authority = "null"; // lub null jeœli lokalny
//        options.RequireHttpsMetadata = true;
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidIssuer = "null",

//            ValidateAudience = true,
//            ValidAudience = "your-client-id",

//            ValidateLifetime = true,
//            ValidateIssuerSigningKey = true
//            // Mo¿esz te¿ rêcznie podaæ IssuerSigningKey
//        };
//    });

//builder.Services.AddScoped(sp =>
//{
//    var httpClient = new HttpClient
//    {
//        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
//    };

//    // Dodaj token rêcznie, np. z localStorage
//    var token = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "jwt_token");
//    if (!string.IsNullOrEmpty(token))
//    {
//        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
//    }

//    return httpClient;
//});

//builder.Services.AddOidcAuthentication(options =>
//{
//    builder.Configuration.Bind("Oidc", options.ProviderOptions);
//});

builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorPolicy", policy =>
        policy.WithOrigins("http://smartapp.local")
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// ... przed UseAuthentication
app.UseCors("BlazorPolicy");

builder.Services.AddOidcAuthentication(options =>
{
    // U¿ywam nazwy Realmu, któr¹ stworzy³em (SmartAppRealm)
    options.ProviderOptions.Authority = "http://keycloak.local/realms/SmartAppRealm";
    options.ProviderOptions.ClientId = "smart-blazor";

    options.ProviderOptions.ResponseType = "code";

    // Te scope'y s¹ wymagane, aby Keycloak zwróci³ poprawne dane u¿ytkownika
    options.ProviderOptions.DefaultScopes.Add("openid");
    options.ProviderOptions.DefaultScopes.Add("profile");

    // Dodam to, jeœli moje API wymaga konkretnego audience
    // options.ProviderOptions.AdditionalProviderParameters.Add("audience", "smart-api");
});

builder.Services.AddRadzenComponents();

builder.Services.AddRadzenCookieThemeService(options =>
{
    options.Name = "MyApplicationTheme";
    options.Duration = TimeSpan.FromDays(365);
});

await builder.Build().RunAsync();
