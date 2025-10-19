using Clinix.Application.Dtos.FollowUp;
using FluentValidation;

namespace Clinix.Application.Validators;

public sealed class CreateFollowUpFromAppointmentRequestValidator : AbstractValidator<CreateFollowUpFromAppointmentRequest>
    {
    public CreateFollowUpFromAppointmentRequestValidator()
        {
        RuleFor(x => x.AppointmentId).GreaterThan(0).WithMessage("AppointmentId must be provided.");
        RuleFor(x => x.CreatedByUserId).GreaterThanOrEqualTo(0);
        RuleFor(x => x.InitiatorNote).MaximumLength(2000);
        }
    }

