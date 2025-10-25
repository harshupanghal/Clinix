using Clinix.Application.Dtos;
using FluentValidation;

namespace Clinix.Application.Validators;
public class LoginModelValidator : AbstractValidator<LoginModel>
    {
    public LoginModelValidator()
        {
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .Matches(@"^\+?\d{7,15}$").WithMessage("Enter a valid phone number.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");
        }
    }
