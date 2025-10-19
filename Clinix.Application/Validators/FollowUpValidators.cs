





//using Clinix.Application.Dtos.FollowUp;
//using FluentValidation;

//namespace Clinix.Application.Validators;

///// <summary>
///// Validator for CreateFollowUpFromAppointmentRequest
///// </summary>
//public class CreateFollowUpFromAppointmentRequestValidator : AbstractValidator<CreateFollowUpFromAppointmentRequest>
//    {
//    public CreateFollowUpFromAppointmentRequestValidator()
//        {
//        RuleFor(x => x.AppointmentId)
//            .GreaterThan(0)
//            .WithMessage("AppointmentId must be a positive integer.");

//        RuleFor(x => x.CreatedByUserId)
//            .GreaterThan(0)
//            .WithMessage("CreatedByUserId must be a positive integer.");

//        RuleFor(x => x.Notes)
//            .MaximumLength(1024)
//            .WithMessage("Notes cannot exceed 1024 characters.");

//        RuleFor(x => x.SuggestedScheduleDays)
//            .Must(list => list == null || list.Count <= 20)
//            .WithMessage("Suggested schedule must be 20 entries or fewer.")
//            .When(x => x.SuggestedScheduleDays != null);

//        // Additional validation: suggested days must be non-negative
//        RuleForEach(x => x.SuggestedScheduleDays)
//            .GreaterThanOrEqualTo(0)
//            .WithMessage("Suggested schedule days must be non-negative.");
//        }
//    }

///// <summary>
///// Validator for FollowUpItemDto if needed for UI operations.
///// </summary>
//public class FollowUpItemDtoValidator : AbstractValidator<FollowUpItemDto>
//    {
//    public FollowUpItemDtoValidator()
//        {
//        RuleFor(x => x.Id).GreaterThan(0);
//        RuleFor(x => x.FollowUpId).GreaterThan(0);
//        RuleFor(x => x.Type).NotEmpty().MaximumLength(64);
//        RuleFor(x => x.Channel).NotEmpty().MaximumLength(32);
//        }
//    }

