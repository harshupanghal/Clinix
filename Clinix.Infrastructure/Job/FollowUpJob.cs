//using Clinix.Application.Interfaces;
//using Clinix.Application.Interfaces.RepoInterfaces;
//using Clinix.Application.Interfaces.ServiceInterfaces;
//using Clinix.Domain.Entities.ApplicationUsers;
//using Clinix.Domain.Entities.FollowUps;
//using Microsoft.Extensions.Logging;

//namespace Clinix.Infrastructure.Job;

//public class FollowUpJob
//    {
//    private readonly IFollowUpRepository _followUpRepo;
//    private readonly IEmailTemplateRepository _templateRepo;
//    private readonly IUserRepository _userRepo;
//    private readonly ICommunicationPreferenceRepository _prefRepo;
//    private readonly INotificationService _notification;
//    private readonly ILogger<FollowUpJob> _log;

//    public FollowUpJob(
//        IFollowUpRepository followUpRepo,
//        IEmailTemplateRepository templateRepo,
//        IUserRepository userRepo,
//        ICommunicationPreferenceRepository prefRepo,
//        INotificationService notification,
//        ILogger<FollowUpJob> log)
//        {
//        _followUpRepo = followUpRepo;
//        _templateRepo = templateRepo;
//        _userRepo = userRepo;
//        _prefRepo = prefRepo;
//        _notification = notification;
//        _log = log;
//        }

//    /// <summary>
//    /// Hangfire-executed job that sends the follow-up.
//    /// </summary>
///*    public async Task SendFollowUpAsync(long followUpId, CancellationToken ct)
//        {
//        var f = await _followUpRepo.GetByIdAsync(followUpId, ct);
//        if (f == null) return;

//        if (f.Status != FollowUpStatus.Scheduled)
//            {
//            _log.LogInformation("FollowUp {id} not scheduled (status {status})", followUpId, f.Status);
//            return;
//            }

//        var pref = await _prefRepo.GetByPatientAsync(f.PatientUserId, ct);
//        if (pref == null || !pref.EmailOptIn)
//            {
//            f.Status = FollowUpStatus.Cancelled;
//            f.UpdatedAt = DateTime.UtcNow;
//            await _followUpRepo.UpdateAsync(f, ct);
//            return;
//            }

//        var user;
//            //= await _userRepo.GetByIdAsync(f.PatientUserId, ct);
//        if (user == null || string.IsNullOrEmpty(user.Email))
//            {
//            f.Status = FollowUpStatus.Failed;
//            f.LastError = "No email on user";
//            f.UpdatedAt = DateTime.UtcNow;
//            await _followUpRepo.UpdateAsync(f, ct);
//            return;
//            }

//        var template = await _templateRepo.GetByNameAsync(f.TemplateName ?? "", ct);
//        var subject = template?.Subject ?? "How are you after your visit?";
//        var body = template?.BodyHtml ?? $"Hello {user.Username},<br/> Hope you're doing well after your visit.";

//        // Here you may want to do template variable replacement (example)
//        body = body.Replace("{{PatientName}}", user.Username);

//        try
//            {
//            await _notification.SendEmailAsync(user.Email, subject, body, ct);
//            f.Status = FollowUpStatus.Sent;
//            f.SentAtUtc = DateTime.UtcNow;
//            f.UpdatedAt = DateTime.UtcNow;
//            await _followUpRepo.UpdateAsync(f, ct);
//            }
//        catch (Exception ex)
//            {
//            _log.LogError(ex, "Failed to send followup {id}", followUpId);
//            f.Status = FollowUpStatus.Failed;
//            f.LastError = ex.Message;
//            f.UpdatedAt = DateTime.UtcNow;
//            await _followUpRepo.UpdateAsync(f, ct);
//            }
//        }
//    }

//*/