using Clinix.Application.Dtos;
using FluentValidation;

namespace Clinix.Application.Validators
    {
    public class PatientUpdateProfileValidator : AbstractValidator<PatientUpdateProfileRequest>
        {
        public PatientUpdateProfileValidator()
            {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("Invalid user identifier.");

            RuleFor(x => x.FullName)
                .MinimumLength(3).When(x => !string.IsNullOrWhiteSpace(x.FullName))
                .WithMessage("Full name must be at least 3 characters when provided.");

            RuleFor(x => x.Email)
                .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
                .WithMessage("Invalid email address.");

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
    }
