using Clinix.Application.Dtos.Appointment;
using FluentValidation;

namespace Clinix.Application.Validators;

public class CreateAppointmentRequestValidator : AbstractValidator<CreateAppointmentRequest>
    {
    public CreateAppointmentRequestValidator()
        {
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.StartAt).LessThan(x => x.EndAt).WithMessage("Start must be before End");
        RuleFor(x => x.EndAt).GreaterThan(x => x.StartAt);
        RuleFor(x => x.StartAt).GreaterThan(DateTimeOffset.UtcNow.AddMinutes(-5)).WithMessage("Start time must be in the future");
        }
    }
