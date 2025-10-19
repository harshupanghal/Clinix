using Blazored.Toast;
using Clinix.Application.Interfaces.Functionalities;
using Clinix.Application.Interfaces.Services;
using Clinix.Application.Interfaces.UserRepo;
using Clinix.Application.Services;
using Clinix.Application.UseCases;
using Clinix.Application.Validators;
using Clinix.Domain.Entities.ApplicationUsers;
using Clinix.Domain.Entities.Appointments;
using Clinix.Infrastructure.Data;
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
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

builder.Services.AddSignalR();

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
builder.Services.AddScoped<IPatientDashboardService, PatientDashboardService>();

builder.Services.AddScoped<IRegistrationService, RegistrationService>();


builder.Services.AddScoped<IAuthenticationService, Clinix.Infrastructure.Services.AuthenticationService>();

builder.Services.AddScoped<IRegistrationUiService, RegistrationUiService>();
builder.Services.AddScoped<ISafeNavigationService, SafeNavigationService>();
builder.Services.AddScoped<IPatientDashboardUiService, PatientDashboardUiService>();

// Repositories
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();

builder.Services.AddScoped<IAppointmentRepository, EfAppointmentRepository>();
builder.Services.AddScoped<IDoctorScheduleRepository, EfDoctorScheduleRepository>();
builder.Services.AddScoped<ISymptomMappingRepository, EfSymptomMappingRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<BookAppointmentUseCase>();
builder.Services.AddScoped<ApproveRejectAppointmentUseCase>();
builder.Services.AddScoped<DelayCascadeUseCase>();
builder.Services.AddScoped<NotifyAppointmentChangeUseCase>();
// Infrastructure services
builder.Services.AddScoped<INotificationDispatcher, InMemoryDispatcher>();
builder.Services.AddHostedService<FollowUpTaskScheduler>();

// Repositories (already above)
builder.Services.AddScoped<IFollowUpTaskRepository, FollowUpTaskRepository>();
builder.Services.AddScoped<IFollowUpService, FollowUpService>();


builder.Services.AddBlazoredToast();
// AutoMapper
builder.Services.AddAutoMapper(typeof(Clinix.Application.Mappings.FollowUpMappingProfile).Assembly);

// FluentValidation - if using
builder.Services.AddValidatorsFromAssembly(typeof(CreateFollowUpFromAppointmentRequestValidator).Assembly);

// Application services
builder.Services.AddScoped<CreateFollowUpFromAppointmentHandler>();

builder.Services.AddScoped<IAppointmentClinicalInfoRepository, AppointmentClinicalInfoRepository>();
builder.Services.AddScoped<IFollowUpRepository, FollowUpRepository>();
builder.Services.AddScoped<IFollowUpTaskRepository, FollowUpTaskRepository>();

builder.Services.AddFluentValidationAutoValidation();   
builder.Services.AddFluentValidationClientsideAdapters(); 

builder.Services.AddValidatorsFromAssemblyContaining<RegisterPatientRequestValidator>();

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
builder.Services.AddMudServices();

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

app.MapHub<NotificationHub>("/notificationhub");


// Seed Director
using (var scope = app.Services.CreateScope())
    {
    var db = scope.ServiceProvider.GetRequiredService<ClinixDbContext>();
    db.Database.Migrate();
    await DataSeeder.SeedAsync(scope.ServiceProvider);
    }

app.Run();





//await _notifyUseCase.NotifyAsync(patientId.ToString(), $"Your appointment with Dr. {doctor.FullName} at {appointment.StartAt:HH:mm} has been {appointment.Status}.");
//await _notifyUseCase.NotifyAsync(doctorId.ToString(), $"Appointment with patient {patient.FullName} at {appointment.StartAt:HH:mm} has been {appointment.Status}.");
