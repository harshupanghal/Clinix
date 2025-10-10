using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Clinix.Application.Dtos;
using FluentValidation;

namespace Clinix.Application.Validators
    {
    public class RegisterPatientRequestValidator : AbstractValidator<RegisterPatientRequest>
        {
        public RegisterPatientRequestValidator()
            {
            // Username
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Fullname is required.")
                .MinimumLength(3).WithMessage("Fullname must be at least 3 characters.")
                .MaximumLength(200).WithMessage("Fullname cannot exceed 200 characters.")
                .Matches("^[a-zA-Z0-9_]+$").WithMessage("Fullname can contain only letters, numbers, and underscores.");

            // Email
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email address.");

            // Phone Number
            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^\+?\d{0,3}?[- .]?\(?\d{1,4}?\)?[- .]?\d{1,9}([- .]?\d{1,9})?$")
                .WithMessage("Invalid phone number format.");

            // Password
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"\d").WithMessage("Password must contain at least one number.")
                .Matches("[!@#$%^&*(),.?\":{ }|<>]").WithMessage("Password must contain at least one special character.");

            // Date of Birth
            RuleFor(x => x.DateOfBirth)
                .NotNull().WithMessage("Date of birth is required.")
                .LessThan(DateTime.Today).WithMessage("Date of birth cannot be in the future.")
                .Must(BeAtLeastOneYearOld).WithMessage("Patient must be at least 1 year old.");

            // Gender
            RuleFor(x => x.Gender)
                .NotEmpty().WithMessage("Gender is required.")
                .Must(BeValidGender).WithMessage("Gender must be 'Male', 'Female', or 'Other'.");

            // Blood Group
            RuleFor(x => x.BloodGroup)
                .NotEmpty().WithMessage("Blood group is required.")
                .Must(BeValidBloodGroup).WithMessage("Invalid blood group.");

            // Emergency Contact
            RuleFor(x => x.EmergencyContact)
                .NotEmpty().WithMessage("Emergency contact is required.")
                .Matches(@"^[0-9]{10,15}$").WithMessage("Emergency contact must be a valid phone number (10–15 digits).");
            }

        private bool BeAtLeastOneYearOld(DateTime? dateOfBirth)
            {
            if (dateOfBirth == null) return false;
            return dateOfBirth.Value <= DateTime.Today.AddYears(-1);
            }

        private bool BeValidGender(string? gender)
            {
            if (string.IsNullOrWhiteSpace(gender)) return false;
            var validGenders = new[] { "Male", "Female", "Other" };
            return Array.Exists(validGenders, g =>
                string.Equals(g, gender, StringComparison.OrdinalIgnoreCase));
            }

        private bool BeValidBloodGroup(string? bloodGroup)
            {
            if (string.IsNullOrWhiteSpace(bloodGroup)) return false;
            var validGroups = new[]
            {
                "A+", "A-", "B+", "B-", "O+", "O-", "AB+", "AB-"
            };
            return Array.Exists(validGroups, g =>
                string.Equals(g, bloodGroup, StringComparison.OrdinalIgnoreCase));
            }
        }
    }
