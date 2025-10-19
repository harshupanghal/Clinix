using Clinix.Application.Dtos.FollowUps;
using FluentValidation;

namespace Clinix.Application.Validators;

public sealed class AdminTaskActionRequestValidator : AbstractValidator<AdminTaskActionRequest>
    {
    public AdminTaskActionRequestValidator()
        {
        RuleFor(x => x.TaskId).GreaterThan(0);
        RuleFor(x => x.ActorUserId).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ActorRole).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(1000);
        }
    }

