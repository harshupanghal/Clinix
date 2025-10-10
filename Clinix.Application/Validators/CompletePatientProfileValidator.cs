using Clinix.Application.Dtos;
using FluentValidation;

namespace Clinix.Application.Validators;

public class CompletePatientProfileValidator : AbstractValidator<CompletePatientProfileRequest>
    {
    public CompletePatientProfileValidator()
        {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("Invalid user identifier.");

        RuleFor(x => x.Gender)
            .Must(x => x == null || new[] { "Male", "Female", "Other" }.Contains(x))
            .WithMessage("Gender must be 'Male', 'Female' or 'Other' if provided.");

        RuleFor(x => x.BloodGroup)
            .Must(x => x == null || new[] { "A+", "A-", "B+", "B-", "O+", "O-", "AB+", "AB-" }.Contains(x))
            .WithMessage("Invalid blood group.");

        RuleFor(x => x.EmergencyContactNumber)
            .Matches(@"^\+?\d{10,15}$")
            .When(x => !string.IsNullOrWhiteSpace(x.EmergencyContactNumber))
            .WithMessage("Invalid emergency contact number.");
        }
    }

