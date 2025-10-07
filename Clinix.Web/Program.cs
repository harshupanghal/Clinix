using System.Net.Http;
using System.Security.Claims;
using Clinix.Application.Interfaces;
using Clinix.Application.Services;
using Clinix.Infrastructure.Data;
using Clinix.Infrastructure.Repositories;
using Clinix.Infrastructure.Services;   // <-- AuthenticationService
using Clinix.Web.Components;
using Clinix.Web.Helpers;
using Clinix.Web.Services;
using FluentValidation;
using Hms.Application.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();
// -------------------------
// Database (SQL Server)
// -------------------------
var connectionString = builder.Configuration.GetConnectionString("ClinixConnection")
                       ?? "Server=(localdb)\\mssqllocaldb;Database=ClxDb;Trusted_Connection=True;";

builder.Services.AddDbContext<ClinixDbContext>(options =>
    options.UseSqlServer(connectionString));

// -------------------------
// Application / Infrastructure DI
// -------------------------
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IInventoryService,InventoryService>();

builder.Services.AddScoped<IRegistrationService, RegistrationService>();

// Authentication service (application -> infrastructure)
builder.Services.AddScoped<Clinix.Application.Interfaces.IAuthenticationService, Clinix.Infrastructure.Services.AuthenticationService>();

// WebUI thin services
builder.Services.AddScoped<IRegistrationUiService, RegistrationUiService>();
builder.Services.AddScoped<ISafeNavigationService, SafeNavigationService>();
//builder.Services.AddSingleton<IPendingAuthService, PendingAuthService>();

// -------------------------
// Antiforgery (register service; we will use header-based tokens from client)
// -------------------------
builder.Services.AddAntiforgery(options =>
{
    // choose a header name your JS or HttpClient will send on state-changing requests
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.IsEssential = true;
});

// -------------------------
// HttpClient for server-side components (Blazor Server)
// -------------------------
// Use HostEnvironment.BaseAddress so HttpClient has correct base for API calls.
builder.Services.AddScoped(sp => new HttpClient
    {
    BaseAddress = new Uri(builder.Configuration["AppBaseAddress"]
        ?? "https://localhost:5001/") // default for dev
    });


// Authentication
const string AuthScheme = "clx-auth";
const string AuthCookie = "clx-cookie";

builder.Services.AddAuthentication(AuthScheme)
    .AddCookie(AuthScheme, options =>
    {
        options.Cookie.Name = AuthCookie;
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/access-denied";
        options.LogoutPath = "/logout";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.IsEssential = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(1);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();


// -------------------------
// Blazor Server Components
// -------------------------
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

// Add controllers so we can expose /api/account/login and /api/account/logout
builder.Services.AddControllers();



// -------------------------
// Build App
// -------------------------
var app = builder.Build();

// -------------------------
// Middleware
// -------------------------
if (!app.Environment.IsDevelopment())
    {
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    }

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Authentication & Authorization middleware (required for cookie auth)
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// -------------------------
// Map controllers + Blazor
// -------------------------
app.MapControllers();


app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();


// -------------------------
// Seed Director
// -------------------------
// Ensure DB and seed admin if configured
using (var scope = app.Services.CreateScope())
    {
    var db = scope.ServiceProvider.GetRequiredService<ClinixDbContext>();
    db.Database.Migrate();
    await DataSeeder.SeedAdminAsync(scope.ServiceProvider);
    }

// -------------------------
// Run App
// -------------------------
app.Run();
