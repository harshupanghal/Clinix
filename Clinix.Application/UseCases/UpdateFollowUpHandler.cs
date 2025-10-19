using Clinix.Application.Dtos.FollowUps;
using Clinix.Application.Interfaces.Functionalities;
using Clinix.Domain.Entities.FollowUps;
using Microsoft.Extensions.Logging;

namespace Clinix.Application.UseCases;

public sealed class UpdateFollowUpHandler
    {
    private readonly IFollowUpRepository _repo;
    private readonly ILogger<UpdateFollowUpHandler> _logger;

    public UpdateFollowUpHandler(IFollowUpRepository repo, ILogger<UpdateFollowUpHandler> logger)
        {
        _repo = repo;
        _logger = logger;
        }

    public async Task HandleAsync(AdminFollowUpUpdateRequest req)
        {
        if (req == null) throw new ArgumentNullException(nameof(req));
        if (req.ActorRole != "Admin") throw new UnauthorizedAccessException("Only admins may perform this action.");

        var followUp = await _repo.GetByIdAsync(req.FollowUpId);
        if (followUp == null) throw new InvalidOperationException("Follow-up not found.");

        if (!string.IsNullOrWhiteSpace(req.DiagnosisSummary) || !string.IsNullOrWhiteSpace(req.Notes))
            {
            // update fields
            followUp.AddNote($"admin:{req.ActorUserId}", req.Notes ?? string.Empty);
            // For diagnosis we directly set via reflection because FollowUpRecord has no public setter for DiagnosisSummary.
            var diagProp = typeof(FollowUpRecord).GetProperty("DiagnosisSummary");
            diagProp?.SetValue(followUp, req.DiagnosisSummary ?? followUp.DiagnosisSummary);
            }

        if (req.AssignDoctorId != null && req.AssignDoctorId > 0)
            {
            var docProp = typeof(FollowUpRecord).GetProperty("DoctorId");
            docProp?.SetValue(followUp, req.AssignDoctorId);
            }

        // persist update - repository currently has only Add & Get; for update we use Update pattern:
        // simplest: re-attach and save via EF repository implementation (add UpdateAsync)
        // For now we assume repository has UpdateAsync (implement below)
        if (_repo is IFollowUpRepositoryExtended ext)
            {
            await ext.UpdateAsync(followUp);
            }
        else
            {
            throw new InvalidOperationException("FollowUpRepository must implement update capability.");
            }

        _logger.LogInformation("Admin {User} updated follow-up {FollowUpId}", req.ActorUserId, req.FollowUpId);
        }
    }

