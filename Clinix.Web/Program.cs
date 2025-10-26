using Blazored.Toast;
using Clinix.Application.Interfaces;
using Clinix.Application.Interfaces.Functionalities;
using Clinix.Application.Interfaces.UserRepo;
using Clinix.Application.Options;
using Clinix.Application.Services;
using Clinix.Application.Validators;
using Clinix.Domain.Interfaces;
using Clinix.Infrastructure.Background;
using Clinix.Infrastructure.Contacts;
using Clinix.Infrastructure.Data;
using Clinix.Infrastructure.Messaging;
using Clinix.Infrastructure.Persistence;
using Clinix.Infrastructure.Repositories;
using Clinix.Infrastructure.Services;
using Clinix.Web.Components;
using Clinix.Web.Helpers;
using Clinix.Web.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Core services
builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddCircuitOptions(options =>
    {
        options.DetailedErrors = true;
    });

builder.Services.AddSignalR();

// Database
var connectionString = builder.Configuration.GetConnectionString("ClinixConnection")
                       ?? "Server=(localdb)\\mssqllocaldb;Database=ClxDb;Trusted_Connection=True;";

builder.Services.AddDbContext<ClinixDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<ISeedStatusRepository, SeedStatusRepository>();

// Options configuration for appointments/follow-ups
builder.Services.Configure<NotificationsOptions>(builder.Configuration.GetSection("Notifications"));
builder.Services.Configure<ReminderOptions>(builder.Configuration.GetSection("Reminders"));

// Domain repositories (existing)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// NEW: Appointment/Follow-up repositories
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IFollowUpRepository, FollowUpRepository>();
builder.Services.AddScoped<IProviderRepository, ProviderRepository>();
builder.Services.AddScoped<IDoctorScheduleRepository, DoctorScheduleRepository>();

// Application services (existing)
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IPatientDashboardService, PatientDashboardService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IAuthenticationService, Clinix.Infrastructure.Services.AuthenticationService>();

// NEW: Appointment/Follow-up application services
builder.Services.AddScoped<IAppointmentAppService, AppointmentAppService>();
builder.Services.AddScoped<IFollowUpAppService, FollowUpAppService>();
builder.Services.AddScoped<IProviderAppService, ProviderAppService>();
builder.Services.AddScoped<IDoctorActionsAppService, DoctorActionsAppService>();
builder.Services.AddScoped<IAdminScheduleAppService, AdminScheduleAppService>();


// UI services (existing)
builder.Services.AddScoped<IRegistrationUiService, RegistrationUiService>();
builder.Services.AddScoped<ISafeNavigationService, SafeNavigationService>();
builder.Services.AddScoped<IPatientDashboardUiService, PatientDashboardUiService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();


// NEW: Notification and background services
builder.Services.AddSingleton<INotificationSender, RealNotificationSender>();
builder.Services.AddSingleton<IContactProvider, FakeContactProvider>();
builder.Services.AddHostedService<FollowUpReminderWorker>();

// Toast notifications
builder.Services.AddBlazoredToast();

// Validation
builder.Services.AddValidatorsFromAssemblyContaining<LoginModelValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterPatientRequestValidator>();

// Antiforgery
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.IsEssential = true;
});

// HTTP client
builder.Services.AddScoped(sp => new HttpClient
    {
    BaseAddress = new Uri(builder.Configuration["AppBaseAddress"] ?? "https://localhost:5001/")
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

// Additional services
builder.Services.AddRazorPages();
builder.Services.AddMudServices();
builder.Services.AddControllers();

var app = builder.Build();

// Middleware
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

using (var scope = app.Services.CreateScope())
    {
    try
        {
        await DataSeeder.SeedAsync(scope.ServiceProvider);
        }
    catch (Exception ex)
        {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogCritical(ex, "Fatal error during database seeding.");
     
        }
    }

app.Run();




//await _notifyUseCase.NotifyAsync(patientId.ToString(), $"Your appointment with Dr. {doctor.FullName} at {appointment.StartAt:HH:mm} has been {appointment.Status}.");
//await _notifyUseCase.NotifyAsync(doctorId.ToString(), $"Appointment with patient {patient.FullName} at {appointment.StartAt:HH:mm} has been {appointment.Status}.");
