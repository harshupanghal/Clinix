using Clinix.Application.Dtos.FollowUps;
using FluentValidation;

namespace Clinix.Application.Validators;

public sealed class AdminFollowUpUpdateRequestValidator : AbstractValidator<AdminFollowUpUpdateRequest>
    {
    public AdminFollowUpUpdateRequestValidator()
        {
        RuleFor(x => x.FollowUpId).GreaterThan(0);
        RuleFor(x => x.ActorUserId).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ActorRole).NotEmpty();
        RuleFor(x => x.DiagnosisSummary).MaximumLength(4000);
        RuleFor(x => x.Notes).MaximumLength(4000);
        }
    }

