//using FluentValidation;
//using Clinix.Application.Dtos;
//using Clinix.Application.Interfaces;

//namespace Clinix.Application.Validators;
//public class RegisterPatientValidator : AbstractValidator<RegisterPatientRequest>
//    {
//    public RegisterPatientValidator(IUserRepository userRepository)
//        {
//        RuleFor(x => x.UserName).NotEmpty().MaximumLength(50)
//            .MustAsync(async (u, ct) => await userRepository.GetByUsernameAsync(u) == null)
//            .WithMessage("Username is already taken.");

//        RuleFor(x => x.Email).NotEmpty().EmailAddress()
//            .MustAsync(async (e, ct) => await userRepository.GetByEmailAsync(e) == null)
//            .WithMessage("Email is already taken.");

//        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
//            .Matches("[A-Z]").WithMessage("Password must contain uppercase.")
//            .Matches("[a-z]").WithMessage("Password must contain lowercase.")
//            .Matches("\\d").WithMessage("Password must contain a digit.")
//            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a special character.");

//        RuleFor(x => x.FirstName).NotEmpty();
//        RuleFor(x => x.LastName).NotEmpty();
//        }
//    }
