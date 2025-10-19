//using Clinix.Application.Dtos.FollowUp;
//using Clinix.Application.Interfaces.Functionalities;
//using Clinix.Application.Services;
//using Clinix.Application.Utilities;
//using Clinix.Domain.Entities.FollowUp;
//using Microsoft.Extensions.Logging;

//namespace Clinix.Application.UseCases
//    {
//    /// <summary>
//    /// Worker use-case: processes due followup items and sends messages using IMessagingService.
//    /// Designed to be idempotent and safe for concurrent workers.
//    /// </summary>
//    public class ProcessDueFollowUpItemsUseCase : IProcessDueFollowUpItemsUseCase
//        {
//        private readonly IFollowUpRepository _followUpRepository;
//        private readonly IMessagingService _messagingService;
//        private readonly ITemplateRenderer _templateRenderer;
//        private readonly ITemplateRepository _templateRepository; // small repo to fetch template bodies
//        private readonly ILogger<ProcessDueFollowUpItemsUseCase> _logger;

//        public ProcessDueFollowUpItemsUseCase(
//            IFollowUpRepository followUpRepository,
//            IMessagingService messagingService,
//            ITemplateRenderer templateRenderer,
//            ITemplateRepository templateRepository,
//            ILogger<ProcessDueFollowUpItemsUseCase> logger)
//            {
//            _followUpRepository = followUpRepository ?? throw new ArgumentNullException(nameof(followUpRepository));
//            _messagingService = messagingService ?? throw new ArgumentNullException(nameof(messagingService));
//            _templateRenderer = templateRenderer ?? throw new ArgumentNullException(nameof(templateRenderer));
//            _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
//            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//            }

//        /// <inheritdoc />
//        public async Task<int> HandleAsync(int maxItems, CancellationToken ct = default)
//            {
//            // fetch due items (repository handles locking/selection semantics)
//            var now = DateTime.UtcNow;
//            var items = (await _followUpRepository.GetDueItemsAsync(now, maxItems, ct)).ToList();

//            if (!items.Any())
//                {
//                _logger.LogDebug("No due followup items found at {Now}", now);
//                return 0;
//                }

//            var processed = 0;

//            foreach (var item in items)
//                {
//                ct.ThrowIfCancellationRequested();

//                try
//                    {
//                    // Defensive: skip deleted items
//                    if (item.IsDeleted)
//                        {
//                        _logger.LogDebug("Skipping deleted FollowUpItem {ItemId}", item.Id);
//                        continue;
//                        }

//                    // Acquire optimistic idempotency by checking status before processing.
//                    // (Repository.UpdateItemAsync should use row-version or transaction to ensure status transition).
//                    if (item.Status != FollowUpItemStatus.Pending)
//                        {
//                        _logger.LogDebug("Skipping non-pending FollowUpItem {ItemId} with status {Status}", item.Id, item.Status);
//                        continue;
//                        }

//                    // Build processing model: repository may provide method to get enriched model (patient contact info, appointment data)
//                    var model = await BuildProcessingModelAsync(item, ct);
//                    if (model == null)
//                        {
//                        _logger.LogWarning("Processing model null for FollowUpItem {ItemId}; marking Skipped", item.Id);
//                        item.Status = FollowUpItemStatus.Skipped;
//                        item.FailureReason = "Missing processing model (no contact or followup)";
//                        await _followUpRepository.UpdateItemAsync(item, ct);
//                        await _followUpRepository.AddAuditAsync(new FollowUpAudit
//                            {
//                            EntityType = nameof(FollowUpItem),
//                            EntityId = item.Id,
//                            Action = "Skipped_NoModel",
//                            TimestampUtc = DateTime.UtcNow,
//                            ActorUserId = item.CreatedByUserId,
//                            DetailsJson = "{}"
//                            }, ct);
//                        continue;
//                        }

//                    // Fetch template if present
//                    string subject = string.Empty;
//                    string body = string.Empty;
//                    if (model.TemplateId.HasValue)
//                        {
//                        var tpl = await _templateRepository.GetByIdAsync(model.TemplateId.Value, ct);
//                        if (tpl == null)
//                            {
//                            _logger.LogWarning("Template not found TemplateId={TemplateId} for Item {ItemId}", model.TemplateId, item.Id);
//                            // fallback: mark failed (permanent) to avoid indefinite retry
//                            item.Status = FollowUpItemStatus.Failed;
//                            item.FailureReason = "TemplateNotFound";
//                            await _followUpRepository.UpdateItemAsync(item, ct);
//                            await _followUpRepository.AddAuditAsync(new FollowUpAudit
//                                {
//                                EntityType = nameof(FollowUpItem),
//                                EntityId = item.Id,
//                                Action = "Failed_TemplateMissing",
//                                TimestampUtc = DateTime.UtcNow,
//                                ActorUserId = item.CreatedByUserId,
//                                DetailsJson = "{}"
//                                }, ct);
//                            continue;
//                            }

//                        subject = tpl.Subject ?? string.Empty;
//                        body = _templateRenderer.Render(tpl.Body, model.TemplateModel ?? new Dictionary<string, object?>());
//                        }
//                    else
//                        {
//                        // if no template set, use generic message body
//                        body = $"You have a follow-up: type={model.Type}. Please login to Clinix to view details.";
//                        }

//                    // Attempt send (IMessagingService returns success/failure and transient flag)
//                    var to = model.ToContactValue ?? string.Empty;
//                    if (string.IsNullOrWhiteSpace(to) && model.Channel != "InApp")
//                        {
//                        // Can't send external message without contact
//                        _logger.LogWarning("No contact found for Item {ItemId}. Channel={Channel}", item.Id, model.Channel);
//                        item.Status = FollowUpItemStatus.Failed;
//                        item.FailureReason = "MissingContact";
//                        await _followUpRepository.UpdateItemAsync(item, ct);
//                        await _followUpRepository.AddAuditAsync(new FollowUpAudit
//                            {
//                            EntityType = nameof(FollowUpItem),
//                            EntityId = item.Id,
//                            Action = "Failed_NoContact",
//                            TimestampUtc = DateTime.UtcNow,
//                            ActorUserId = item.CreatedByUserId,
//                            DetailsJson = "{}"
//                            }, ct);
//                        continue;
//                        }

//                    MessagingResult sendResult;
//                    if (model.Channel == "InApp")
//                        {
//                        // For in-app messages, treat persistence as 'send' by creating an in-app notification; use messaging service adapter for InApp
//                        sendResult = await _messagingService.SendAsync("InApp", to, subject, body, ct);
//                        }
//                    else
//                        {
//                        sendResult = await _messagingService.SendAsync(model.Channel, to, subject, body, ct);
//                        }

//                    // Update item based on sendResult
//                    item.AttemptCount++;
//                    item.LastAttemptedAtUtc = DateTime.UtcNow;
//                    if (sendResult.Success)
//                        {
//                        item.Status = FollowUpItemStatus.Sent;
//                        item.SentAtUtc = DateTime.UtcNow;
//                        item.FailureReason = null;
//                        item.ResultMetadataJson = $"{{\"providerMessageId\":\"{sendResult.ProviderMessageId}\"}}";
//                        await _followUpRepository.UpdateItemAsync(item, ct);

//                        await _followUpRepository.AddAuditAsync(new FollowUpAudit
//                            {
//                            EntityType = nameof(FollowUpItem),
//                            EntityId = item.Id,
//                            Action = "Sent",
//                            TimestampUtc = DateTime.UtcNow,
//                            ActorUserId = item.CreatedByUserId,
//                            DetailsJson = $"{{\"providerId\":\"{sendResult.ProviderMessageId}\"}}"
//                            }, ct);
//                        }
//                    else
//                        {
//                        // Failure — decide retry or permanent fail
//                        item.FailureReason = TruncateForLog(sendResult.FailureReason, 500);

//                        var isTransient = sendResult.IsTransientFailure;
//                        if (isTransient && item.AttemptCount < item.MaxAttempts)
//                            {
//                            // schedule next attempt
//                            var delay = RetryBackoff.ComputeNextDelay(item.AttemptCount);
//                            item.NextAttemptAtUtc = DateTime.UtcNow.Add(delay);
//                            item.Status = FollowUpItemStatus.Pending;
//                            }
//                        else
//                            {
//                            // permanent failure
//                            item.Status = FollowUpItemStatus.Failed;
//                            }

//                        await _followUpRepository.UpdateItemAsync(item, ct);

//                        await _followUpRepository.AddAuditAsync(new FollowUpAudit
//                            {
//                            EntityType = nameof(FollowUpItem),
//                            EntityId = item.Id,
//                            Action = isTransient ? "RetryScheduled" : "Failed",
//                            TimestampUtc = DateTime.UtcNow,
//                            ActorUserId = item.CreatedByUserId,
//                            DetailsJson = $"{{\"reason\":\"{JsonEscape(item.FailureReason)}\",\"isTransient\":{isTransient.ToString().ToLowerInvariant()}}}"
//                            }, ct);
//                        }

//                    processed++;
//                    }
//                catch (Exception ex)
//                    {
//                    _logger.LogError(ex, "Unhandled error while processing FollowUpItem {ItemId}", item.Id);

//                    try
//                        {
//                        item.AttemptCount++;
//                        item.LastAttemptedAtUtc = DateTime.UtcNow;
//                        item.FailureReason = TruncateForLog(ex.Message, 500);
//                        // mark transient and schedule retry if attempts remain
//                        if (item.AttemptCount < item.MaxAttempts)
//                            {
//                            var delay = RetryBackoff.ComputeNextDelay(item.AttemptCount);
//                            item.NextAttemptAtUtc = DateTime.UtcNow.Add(delay);
//                            item.Status = FollowUpItemStatus.Pending;
//                            }
//                        else
//                            {
//                            item.Status = FollowUpItemStatus.Failed;
//                            }
//                        await _followUpRepository.UpdateItemAsync(item, ct);
//                        await _followUpRepository.AddAuditAsync(new FollowUpAudit
//                            {
//                            EntityType = nameof(FollowUpItem),
//                            EntityId = item.Id,
//                            Action = "ProcessingException",
//                            TimestampUtc = DateTime.UtcNow,
//                            ActorUserId = item.CreatedByUserId,
//                            DetailsJson = JsonEscape(ex.Message)
//                            }, ct);
//                        }
//                    catch (Exception inner)
//                        {
//                        _logger.LogError(inner, "Failed to persist FollowUpItem failure state for Item {ItemId}", item.Id);
//                        }
//                    }
//                }

//            return processed;
//            }

//        private static string TruncateForLog(string? value, int maxLen)
//            => string.IsNullOrEmpty(value) ? string.Empty
//               : (value.Length <= maxLen ? value : value.Substring(0, maxLen) + "…");

//        private static string JsonEscape(string? v)
//            => string.IsNullOrEmpty(v) ? "{}" : System.Text.Json.JsonSerializer.Serialize(v);

//        /// <summary>
//        /// Builds a processing model for an item, including patient contact and appointment context.
//        /// The repository should be able to fetch contact details; this method centralizes mapping and basic checks.
//        /// </summary>
//        private async Task<FollowUpItemProcessingModel?> BuildProcessingModelAsync(FollowUpItem item, CancellationToken ct)
//            {
//            // Retrieve followup record to obtain patient id and appointment id
//            var followUp = await _followUpRepository.GetByIdAsync(item.FollowUpId, ct);
//            if (followUp == null) return null;

//            // fetch patient contact via repository (we assume a method exists to fetch preferred contact for channel)
//            var contact = await _followUpRepository.GetPreferredContactForPatientAsync(followUp.PatientId, item.Channel, ct);
//            if (contact == null && item.Channel != "InApp")
//                {
//                return null;
//                }

//            // get appointment/prescription context for the template model
//            Prescription? prescription = null;
//            if (followUp.AppointmentId.HasValue)
//                {
//                prescription = await _followUpRepository.GetPrescriptionForAppointmentAsync(followUp.AppointmentId.Value, ct);
//                }

//            var templateModel = new Dictionary<string, object?>
//                {
//                ["patientId"] = followUp.PatientId,
//                ["followUpId"] = followUp.Id,
//                ["appointmentId"] = followUp.AppointmentId,
//                ["followUpNotes"] = followUp.Notes
//                };

//            if (prescription != null)
//                {
//                templateModel["diagnosis"] = prescription.Diagnosis;
//                templateModel["doctorNotes"] = prescription.DoctorNotes;
//                templateModel["medications"] = prescription.Medications?.Select(m => new { m.Name, m.Dosage, m.Frequency }).ToArray();
//                }

//            return new FollowUpItemProcessingModel
//                {
//                ItemId = item.Id,
//                FollowUpId = followUp.Id,
//                PatientId = followUp.PatientId,
//                AppointmentId = followUp.AppointmentId,
//                Channel = item.Channel,
//                ScheduledAtUtc = item.ScheduledAtUtc,
//                AttemptCount = item.AttemptCount,
//                MaxAttempts = item.MaxAttempts,
//                TemplateId = item.MessageTemplateId,
//                Type = item.Type,
//                ToContactValue = contact?.ContactValue,
//                TemplateModel = templateModel
//                };
//            }
//        }
//    }
