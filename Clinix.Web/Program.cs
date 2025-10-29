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
using Clinix.Infrastructure.Events;
using Clinix.Infrastructure.Messaging;
using Clinix.Infrastructure.Notifications;
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
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddCircuitOptions(options =>
    {
        options.DetailedErrors = builder.Environment.IsDevelopment();
    });

builder.Services.AddSignalR();

var connectionString = builder.Configuration.GetConnectionString("ClinixConnection")
                       ?? "Server=(localdb)\\mssqllocaldb;Database=ClxDb;Trusted_Connection=True;MultipleActiveResultSets=true;";

builder.Services.AddScoped<DomainEventDispatcher>();

builder.Services.AddDbContext<ClinixDbContext>((sp, options) =>
{
    options.UseSqlServer(connectionString);

    var interceptor = new DomainEventSaveChangesInterceptor(sp);
    options.AddInterceptors(interceptor);
});

builder.Services.AddScoped<ISeedStatusRepository, SeedStatusRepository>();

builder.Services.Configure<NotificationsOptions>(builder.Configuration.GetSection("Notifications"));
builder.Services.Configure<ReminderOptions>(builder.Configuration.GetSection("Reminders"));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IFollowUpRepository, FollowUpRepository>();
builder.Services.AddScoped<IProviderRepository, ProviderRepository>();
builder.Services.AddScoped<IDoctorScheduleRepository, DoctorScheduleRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Core services
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IPatientDashboardService, PatientDashboardService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();

// Appointment & Follow-up services
builder.Services.AddScoped<IAppointmentAppService, AppointmentAppService>();
builder.Services.AddScoped<IFollowUpAppService, FollowUpAppService>();
builder.Services.AddScoped<IProviderAppService, ProviderAppService>();
builder.Services.AddScoped<IDoctorActionsAppService, DoctorActionsAppService>();
builder.Services.AddScoped<IAdminScheduleAppService, AdminScheduleAppService>();

// Notification sender (Email/SMS)
builder.Services.AddSingleton<INotificationSender, RealNotificationSender>();

// Contact provider (gets patient/doctor email/phone from DB)
builder.Services.AddScoped<DbContactProvider>();
builder.Services.AddScoped<IContactProvider, DbContactProvider>();

// Notification handlers (processes domain events)
builder.Services.AddScoped<NotificationHandlers>();

// Register workers WITHOUT auto-start (manual start after seeding)
builder.Services.AddSingleton<OutboxProcessorWorker>();
builder.Services.AddSingleton<FollowUpReminderWorker>();

// UI SERVICES
builder.Services.AddScoped<IRegistrationUiService, RegistrationUiService>();
builder.Services.AddScoped<ISafeNavigationService, SafeNavigationService>();
builder.Services.AddScoped<IPatientDashboardUiService, PatientDashboardUiService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// TOAST NOTIFICATIONS
builder.Services.AddBlazoredToast();

// VALIDATION
builder.Services.AddValidatorsFromAssemblyContaining<LoginModelValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterPatientRequestValidator>();

// ANTIFORGERY
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.IsEssential = true;
});

// HTTP CLIENT
builder.Services.AddScoped(sp => new HttpClient
    {
    BaseAddress = new Uri(builder.Configuration["AppBaseAddress"] ?? "https://localhost:5001/")
    });

// AUTHENTICATION & AUTHORIZATION
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

//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
//    options.AddPolicy("DoctorOnly", policy => policy.RequireRole("Doctor"));
//    options.AddPolicy("PatientOnly", policy => policy.RequireRole("Patient"));
//    options.AddPolicy("ReceptionistOnly", policy => policy.RequireRole("Receptionist"));
//    options.AddPolicy("ChemistOnly", policy => policy.RequireRole("Chemist"));
//    options.AddPolicy("StaffOnly", policy => policy.RequireRole("Receptionist", "Chemist"));
//});

builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

builder.Services.AddRazorPages();
builder.Services.AddMudServices();
builder.Services.AddControllers();

// BUILD APPLICATION
var app = builder.Build();

using (var scope = app.Services.CreateScope())
    {
    var seedLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
        {
        seedLogger.LogInformation("🌱 Starting database seeding...");
        await DataSeeder.SeedAsync(scope.ServiceProvider);
        seedLogger.LogInformation("✅ Database seeding completed!");
        }
    catch (Exception ex)
        {
        seedLogger.LogError(ex, "❌ Seeding failed: {Message}", ex.Message);
        // Don't throw - let app start even if seeding fails
        }
    }

var workerLogger = app.Services.GetRequiredService<ILogger<Program>>();
workerLogger.LogInformation("🚀 Starting background workers...");

// Start OutboxProcessorWorker
_ = Task.Run(async () =>
{
    try
        {
        var worker = app.Services.GetRequiredService<OutboxProcessorWorker>();
        await worker.StartAsync(CancellationToken.None);
        workerLogger.LogInformation("✅ OutboxProcessorWorker started");
        }
    catch (Exception ex)
        {
        workerLogger.LogError(ex, "❌ Failed to start OutboxProcessorWorker");
        }
});

// Start FollowUpReminderWorker  
_ = Task.Run(async () =>
{
    try
        {
        var worker = app.Services.GetRequiredService<FollowUpReminderWorker>();
        await worker.StartAsync(CancellationToken.None);
        workerLogger.LogInformation("✅ FollowUpReminderWorker started");
        }
    catch (Exception ex)
        {
        workerLogger.LogError(ex, "❌ Failed to start FollowUpReminderWorker");
        }
});

// MIDDLEWARES
if (!app.Environment.IsDevelopment())
    {
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    }
else
    {
    app.UseDeveloperExceptionPage();
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

workerLogger.LogInformation("🎉 Clinix HMS started successfully!");
app.Run();

