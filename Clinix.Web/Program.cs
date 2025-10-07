using Clinix.Application.Interfaces.RepoInterfaces;
using Clinix.Application.Interfaces.ServiceInterfaces;
using Clinix.Application.Services;
using Clinix.Infrastructure.Data;
using Clinix.Infrastructure.Persistence;
using Clinix.Infrastructure.Repositories;
using Clinix.Infrastructure.Services;
using Clinix.Web.Components;
using Clinix.Web.Helpers;
using Clinix.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

var connectionString = builder.Configuration.GetConnectionString("ClinixConnection")
                       ?? "Server=(localdb)\\mssqllocaldb;Database=ClxDb;Trusted_Connection=True;";

builder.Services.AddDbContext<ClinixDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IInventoryService, InventoryService>();

builder.Services.AddScoped<IRegistrationService, RegistrationService>();

builder.Services.AddScoped<Clinix.Application.Interfaces.ServiceInterfaces.IAuthenticationService, Clinix.Infrastructure.Services.AuthenticationService>();

builder.Services.AddScoped<IRegistrationUiService, RegistrationUiService>();
builder.Services.AddScoped<ISafeNavigationService, SafeNavigationService>();
//builder.Services.AddSingleton<IPendingAuthService, PendingAuthService>();

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped(sp => new HttpClient
    {
    BaseAddress = new Uri(builder.Configuration["AppBaseAddress"]
        ?? "https://localhost:5001/") 
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

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    {
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    }

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapControllers();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

// Seed Director
using (var scope = app.Services.CreateScope())
    {
    var db = scope.ServiceProvider.GetRequiredService<ClinixDbContext>();
    db.Database.Migrate();
    await DataSeeder.SeedAdminAsync(scope.ServiceProvider);
    }

app.Run();
