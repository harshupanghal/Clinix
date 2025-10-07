//using FluentValidation;
//using Clinix.Application.Dtos;
//using Clinix.Application.Interfaces;
//using Clinix.Domain.Entities;

//namespace Clinix.Application.Validators;
//public class RegisterStaffValidator : AbstractValidator<CreateDoctorRequest>
//    {
//    public RegisterStaffValidator(IUserRepository userRepository)
//        {
//        RuleFor(x => x.UserName).NotEmpty()
//            .MustAsync(async (u, ct) => await userRepository.GetByUsernameAsync(u) == null)
//            .WithMessage("Username is already taken.");

//        RuleFor(x => x.Email).NotEmpty().EmailAddress()
//            .MustAsync(async (e, ct) => await userRepository.GetByEmailAsync(e) == null)
//            .WithMessage("Email is already taken.");

//        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);

//        RuleFor(x => x.Role).IsInEnum();

//        When(x => x.Role == Role.Doctor, () =>
//        {
//            RuleFor(x => x.LicenseNumber).NotEmpty().WithMessage("License number required for doctor.");
//            RuleFor(x => x.Specialization).NotEmpty().WithMessage("Specialization required for doctor.");
//        });

//        When(x => x.Role == Role.Chemist, () =>
//        {
//            RuleFor(x => x.PharmacyName).NotEmpty().WithMessage("Pharmacy name required for chemist.");
//            RuleFor(x => x.LicenseNumber).NotEmpty().WithMessage("License number required for chemist.");
//        });

//        When(x => x.Role == Role.Receptionist, () =>
//        {
//            RuleFor(x => x.Department).NotEmpty().WithMessage("Department required for receptionist.");
//            RuleFor(x => x.Shift).NotEmpty().WithMessage("Shift required for receptionist.");
//        });
//        }
//    }
