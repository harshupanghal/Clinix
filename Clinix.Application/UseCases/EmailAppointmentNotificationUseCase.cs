
//namespace Clinix.Application.UseCases;

//public class EmailAppointmentNotificationUseCase
//    {
//    private readonly IEmailService _emailService;


//    public EmailAppointmentNotificationUseCase(IEmailService emailService)
//        {
//        _emailService = emailService;
//        }


//    public async Task NotifyAsync(string email, string subject, string message)
//        {
//        await _emailService.SendEmailAsync(email, subject, message);
//        }
//    }


////// Usage Example (After Appointment Update)
////await _emailUseCase.NotifyAsync(patient.Email, "Appointment Update", $"Your appointment with Dr. {doctor.FullName} at {appointment.StartAt:HH:mm} is {appointment.Status}.");
////    await _emailUseCase.NotifyAsync(doctor.Email, "Appointment Update", $"Appointment with patient {patient.FullName} at {appointment.StartAt:HH:mm} is {appointment.Status}.");


////// Program.cs (DI Registration)
////builder.Services.AddSingleton<IEmailService>(new SmtpEmailService(
////builder.Configuration["SMTP:Host"],
////int.Parse(builder.Configuration["SMTP:Port"]!),
////builder.Configuration["SMTP:User"],
////builder.Configuration["SMTP:Pass"]
////));
////builder.Services.AddScoped<EmailAppointmentNotificationUseCase>();
