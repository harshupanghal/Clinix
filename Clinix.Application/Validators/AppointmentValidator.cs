using Clinix.Application.Dtos;
using FluentValidation;

namespace Clinix.Application.Validators;
public class AppointmentCreateDtoValidator : AbstractValidator<AppointmentCreateDto>
    {
    public AppointmentCreateDtoValidator()
        {
        RuleFor(x => x.PatientId).GreaterThan(0);
        RuleFor(x => x.DoctorId).GreaterThan(0);
        RuleFor(x => x.StartTime).GreaterThan(DateTime.UtcNow)
            .WithMessage("Start time must be in the future.");
        RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime)
            .WithMessage("End time must be after start time.");
        RuleFor(x => x.Reason).MaximumLength(2000);
        RuleFor(x => x.Type).MaximumLength(100);
        }
    }

public class AppointmentUpdateDtoValidator : AbstractValidator<AppointmentUpdateDto>
    {
    public AppointmentUpdateDtoValidator()
        {
        RuleFor(x => x.AppointmentId).GreaterThan(0);
        RuleFor(x => x.NewStartTime).GreaterThan(DateTime.UtcNow);
        RuleFor(x => x.NewEndTime).GreaterThan(x => x.NewStartTime);
        RuleFor(x => x.Status)
            .Must(status => string.IsNullOrEmpty(status) ||
                            new[] { "Scheduled", "Rescheduled", "Cancelled" }.Contains(status))
            .WithMessage("Status must be Scheduled, Rescheduled or Cancelled.");
        RuleFor(x => x.Reason).MaximumLength(2000);
        }
    }

public class DelayAppointmentsDtoValidator : AbstractValidator<DelayAppointmentsDto>
    {
    public DelayAppointmentsDtoValidator()
        {
        RuleFor(x => x.DoctorId).GreaterThan(0);
        RuleFor(x => x.DelayDuration).GreaterThan(TimeSpan.Zero)
            .WithMessage("Delay duration must be greater than zero.");
        }
    }
