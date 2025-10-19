using AutoMapper;
using Clinix.Application.Dtos.FollowUps;
using Clinix.Application.Interfaces.Functionalities;
using Clinix.Domain.Entities.FollowUps;
using Clinix.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Clinix.Application.UseCases;

public sealed class CreateFollowUpFromAppointmentHandler
    {
    private readonly IAppointmentRepository _appointmentRepo;
    private readonly IAppointmentClinicalInfoRepository _clinicalRepo;
    private readonly IFollowUpRepository _followUpRepo;
    private readonly IFollowUpTaskRepository _taskRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateFollowUpFromAppointmentHandler> _logger;

    public CreateFollowUpFromAppointmentHandler(
        IAppointmentRepository appointmentRepo,
        IAppointmentClinicalInfoRepository clinicalRepo,
        IFollowUpRepository followUpRepo,
        IFollowUpTaskRepository taskRepo,
        IMapper mapper,
        ILogger<CreateFollowUpFromAppointmentHandler> logger)
        {
        _appointmentRepo = appointmentRepo;
        _clinicalRepo = clinicalRepo;
        _followUpRepo = followUpRepo;
        _taskRepo = taskRepo;
        _mapper = mapper;
        _logger = logger;
        }

    /// <summary>
    /// Main entry. Creates a follow-up record from appointment and schedules initial follow-up tasks.
    /// This method should be called within an application transaction scope where required.
    /// </summary>
    public async Task<FollowUpDto> HandleAsync(CreateFollowUpFromAppointmentRequest request)
        {
        if (request == null) throw new ArgumentNullException(nameof(request));

        _logger.LogInformation("Creating follow-up for appointment {AppointmentId} by user {UserId}", request.AppointmentId, request.CreatedByUserId);

        // 1. Load appointment
        var appointment = await _appointmentRepo.GetByIdAsync(request.AppointmentId);
        if (appointment == null)
            {
            _logger.LogWarning("Appointment {AppointmentId} not found", request.AppointmentId);
            throw new InvalidOperationException("Appointment not found.");
            }

        // 2. Load clinical info
        var clinical = await _clinicalRepo.GetByAppointmentIdAsync(request.AppointmentId);

        // 3. Create follow-up record from domain model
        var followUp = new FollowUpRecord(
            patientId: appointment.PatientId,
            appointmentId: appointment.Id,
            doctorId: appointment.DoctorId,
            diagnosisSummary: clinical?.DiagnosisSummary,
            notes: request.InitiatorNote ?? clinical?.DoctorNotes,
            prescriptionId: null // no separate prescription entity for now
        );

        // 4. Snapshot medications (if any)
        if (clinical?.Medications != null && clinical.Medications.Any())
            {
            followUp.AddMedicationSnapshots(clinical.Medications);
            }

        // 5. Persist followUp (repository implementation should set Id)
        await _followUpRepo.AddAsync(followUp);

        // 6. Create FollowUpTasks as needed
        var tasksToCreate = new List<FollowUpTask>();

        // Medication reminders: create simple rule-based reminders (example: daily x duration)
        if (request.EnqueueMedicationReminders && clinical?.Medications != null)
            {
            foreach (var med in clinical.Medications)
                {
                // Simple schedule policy sample (production: improve policy)
                // Schedule first reminder at Next day 9AM UTC (example). Real system should compute times from dosing frequency.
                var nextRun = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(9);
                var payload = new
                    {
                    FollowUpId = followUp.Id,
                    Medicine = med.Name,
                    med.Dosage,
                    med.Frequency,
                    med.Duration,
                    Instruction = med.Notes
                    };

                var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);

                // create a single daily reminder (for demo we schedule one; scheduler can expand into series)
                tasksToCreate.Add(new FollowUpTask(followUp.Id, FollowUpTaskType.MedicationReminder, payloadJson, nextRun, maxAttempts: 3));
                }
            }

        // Revisit reminder: use clinical.NextFollowUpDate if present; else skip
        if (request.EnqueueRevisitReminder && clinical?.NextFollowUpDate != null)
            {
            var revisitAt = clinical.NextFollowUpDate.Value;
            var payload = new { FollowUpId = followUp.Id, Reason = "Revisit reminder" };
            var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);
            tasksToCreate.Add(new FollowUpTask(followUp.Id, FollowUpTaskType.RevisitReminder, payloadJson, revisitAt, maxAttempts: 2));
            }

        if (tasksToCreate.Any())
            {
            await _taskRepo.AddManyAsync(tasksToCreate);
            }

        _logger.LogInformation("Follow-up {FollowUpId} created with {TaskCount} tasks", followUp.Id, tasksToCreate.Count);

        var dto = _mapper.Map<FollowUpDto>(followUp);
        return dto;
        }
    }

