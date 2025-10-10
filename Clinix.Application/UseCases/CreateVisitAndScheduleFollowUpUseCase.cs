//using Clinix.Application.Interfaces;
//using Clinix.Application.Interfaces.RepoInterfaces;
//using Clinix.Domain.Entities.ApplicationUsers;
//using Clinix.Domain.Entities.FollowUps;
//using Hangfire;

//namespace Clinix.Application.UseCases
//    {
//    public class CreateVisitAndScheduleFollowUpUseCase
//        {
//        private readonly IVisitRepository _visitRepo;
//        private readonly IFollowUpRepository _followUpRepo;
//        private readonly ICommunicationPreferenceRepository _prefRepo;
//        private readonly IEmailTemplateRepository _templateRepo;
//        private readonly IBackgroundJobClient _jobClient; // Hangfire client

//        public CreateVisitAndScheduleFollowUpUseCase(
//            IVisitRepository visitRepo,
//            IFollowUpRepository followUpRepo,
//            ICommunicationPreferenceRepository prefRepo,
//            IEmailTemplateRepository templateRepo,
//            IBackgroundJobClient jobClient
//            )
//            {
//            _visitRepo = visitRepo;
//            _followUpRepo = followUpRepo;
//            _prefRepo = prefRepo;
//            _templateRepo = templateRepo;
//            _jobClient = jobClient;
//            }

//        /// <summary>
//        /// Create a visit and schedule follow-up if allowed.
//        /// </summary>
//        public async Task<long> ExecuteAsync(Visit visit, CancellationToken ct = default)
//            {
//            // save visit
//            await _visitRepo.AddAsync(visit, ct);

//            if (!visit.AutoScheduleFollowUp) return visit.Id;

//            // check communication preferences
//            var pref = await _prefRepo.GetByPatientAsync(visit.PatientUserId, ct);
//            if (pref == null || !pref.EmailOptIn) // if no pref assume no marketing
//                {
//                return visit.Id;
//                }

//            // select template (could be more advanced)
//            var template = await _templateRepo.GetByNameAsync("followup_basic_3days", ct)
//                           ?? (await _templateRepo.GetAllAsync(ct)).FirstOrDefault();

//            var scheduledAt = visit.VisitDateUtc.AddDays(visit.FollowUpDaysAfterVisit);

//            var followUp = new FollowUp
//                {
//                PatientUserId = visit.PatientUserId,
//                VisitId = visit.Id,
//                ScheduledAtUtc = scheduledAt.ToUniversalTime(), // make sure UTC
//                TemplateName = template?.Name ?? "default_followup",
//                Channel = pref.PreferredChannel ?? "email"
//                };

//            await _followUpRepo.AddAsync(followUp, ct);

//            // schedule Hangfire job to send at scheduledAt
//            var jobId = _jobClient.Schedule<Clinix.Infrastructure.Jobs.FollowUpJob>(
//                job => job.SendFollowUpAsync(followUp.Id, CancellationToken.None),
//                scheduledAt - DateTime.UtcNow);

//            followUp.JobId = jobId;
//            await _followUpRepo.UpdateAsync(followUp, ct);

//            return visit.Id;
//            }
//        }
//    }
