using Clinix.Application.Dto;
using FluentValidation;

namespace Clinix.Application.Validators;

public class CreateAppointmentDtoValidator : AbstractValidator<CreateAppointmentDto>
    {
    public CreateAppointmentDtoValidator()
        {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.PreferredUtc)
            .Must(dt => dt == null || dt.Value > DateTime.UtcNow.AddMinutes(-5))
            .WithMessage("Preferred time must be in the future.");
        // doctorId and slotId can be null; service will attempt match
        }
    }
