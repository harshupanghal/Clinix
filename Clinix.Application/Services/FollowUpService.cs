using AutoMapper;
using Clinix.Application.Dtos.FollowUps;
using Clinix.Application.Interfaces.Functionalities;
using Clinix.Application.Interfaces.Services;
using Clinix.Domain.Entities.Appointments;
using Clinix.Domain.Entities.FollowUps;
using Clinix.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Clinix.Application.Services;

public class FollowUpService : IFollowUpService
    {
    private readonly IFollowUpRepository _followUpRepo;
    private readonly IFollowUpTaskRepository _taskRepo;
    private readonly IAppointmentRepository _appointmentRepo;
    private readonly IAppointmentClinicalInfoRepository _clinicalRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<FollowUpService> _logger;

    public FollowUpService(
        IFollowUpRepository followUpRepo,
        IFollowUpTaskRepository taskRepo,
        IAppointmentRepository appointmentRepo,
        IAppointmentClinicalInfoRepository clinicalRepo,
        IMapper mapper,
        ILogger<FollowUpService> logger)
        {
        _followUpRepo = followUpRepo;
        _taskRepo = taskRepo;
        _appointmentRepo = appointmentRepo;
        _clinicalRepo = clinicalRepo;
        _mapper = mapper;
        _logger = logger;
        }

    public async Task<IEnumerable<FollowUpListItemDto>> GetAllForAdminAsync(CancellationToken cancellationToken = default)
        {
        var list = await _followUpRepo.GetAllAsync(cancellationToken);
        return list.Select(f => new FollowUpListItemDto
            {
            Id = f.Id,
            PatientName = f.Appointment?.Patient?.User?.FullName ?? "Unknown",
            DoctorName = f.Appointment?.Doctor?.User?.FullName ?? "N/A",
            AppointmentDate = f.Appointment?.StartAt ?? DateTimeOffset.MinValue,
            Status = f.Status.ToString(),
            NextFollowUp = f.MedicationSnapshots.Any() ? f.MedicationSnapshots.Min(m => m.CreatedAt) : (DateTimeOffset?)null
            }).ToList();
        }

    public async Task<FollowUpDetailDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        {
        var f = await _followUpRepo.GetByIdAsync(id);
        if (f == null) return null;

        // Get tasks for this follow-up
        var tasks = await _taskRepo.GetTasksForFollowUpAsync(id, cancellationToken);

        return new FollowUpDetailDto
            {
            Id = f.Id,
            PatientName = f.Appointment?.Patient?.User?.FullName ?? "Unknown",
            DoctorName = f.Appointment?.Doctor?.User?.FullName ?? "N/A",
            AppointmentDate = f.Appointment?.StartAt ?? DateTimeOffset.MinValue,
            Diagnosis = f.DiagnosisSummary,
            Notes = f.Notes,
            Tasks = tasks.Select(t => new FollowUpTaskDto
                {
                Id = t.Id,
                Description = t.TaskType.ToString(),
                Payload = t.Payload,
                ScheduledAt = t.ScheduledAt,
                Status = t.Status
                }).ToList()
            };
        }

    public async Task<FollowUpDto> CreateManualFollowUpAsync(CreateManualFollowUpRequest request, CancellationToken cancellationToken = default)
        {
        if (request == null) throw new ArgumentNullException(nameof(request));

        // Validate appointment if provided
        Appointment? appointment = null;
        if (request.AppointmentId.HasValue)
            {
            appointment = await _appointmentRepo.GetByIdAsync(request.AppointmentId.Value);
            if (appointment == null) throw new InvalidOperationException("Appointment not found.");
            }

        var followUp = new FollowUpRecord(
            patientId: request.PatientId,
            appointmentId: request.AppointmentId,
            doctorId: request.DoctorId,
            diagnosisSummary: request.Diagnosis,
            notes: request.Notes
        );

        if (request.Medications != null && request.Medications.Any())
            {
            // convert DTO meds to domain MedicationItem and snapshot them
            var meds = request.Medications.Select(m => new Clinix.Domain.Entities.Appointments.MedicationItem(
                m.Name, m.Dosage ?? "", m.Frequency ?? "", m.Duration ?? "", m.Notes));
            followUp.AddMedicationSnapshots(meds);
            }

        // Persist follow-up first so followUp.Id is available
        await _followUpRepo.AddAsync(followUp);

        // Create tasks if provided
        if (request.Tasks != null && request.Tasks.Any())
            {
            var tasks = request.Tasks.Select(t =>
            {
                // Try parse t.Type into enum if needed, default to ManualAction
                if (!Enum.TryParse<FollowUpTaskType>(t.Type, ignoreCase: true, out var type))
                    type = FollowUpTaskType.ManualAction;

                return new FollowUpTask(followUp.Id, type, t.Payload ?? string.Empty, t.ScheduledAt, t.MaxAttempts);
            }).ToList();

            // Persist tasks
            await _taskRepo.AddManyAsync(tasks);
            }

        return _mapper.Map<FollowUpDto>(followUp);
        }

    public async Task RescheduleTaskAsync(long taskId, DateTimeOffset scheduledAt, long actorUserId, CancellationToken cancellationToken = default)
        {
        var task = await _taskRepo.GetByIdAsync(taskId, cancellationToken);
        if (task == null) throw new InvalidOperationException("Task not found.");

        // use domain method
        task.Reschedule(scheduledAt, $"admin:{actorUserId}");
        await _taskRepo.UpdateAsync(task, cancellationToken);
        }

    public async Task PauseTaskAsync(long taskId, long actorUserId, CancellationToken cancellationToken = default)
        {
        var task = await _taskRepo.GetByIdAsync(taskId, cancellationToken);
        if (task == null) throw new InvalidOperationException("Task not found.");

        task.Cancel($"admin:{actorUserId}", "paused");
        await _taskRepo.UpdateAsync(task, cancellationToken);
        }

    public async Task CancelTaskAsync(long taskId, long actorUserId, string? reason, CancellationToken cancellationToken = default)
        {
        var task = await _taskRepo.GetByIdAsync(taskId, cancellationToken);
        if (task == null) throw new InvalidOperationException("Task not found.");

        task.Cancel($"admin:{actorUserId}", reason);
        await _taskRepo.UpdateAsync(task, cancellationToken);
        }

    public async Task<IEnumerable<FollowUpTaskDto>> GetTasksForDoctorAsync(long doctorId, CancellationToken cancellationToken = default)
        {
        var tasks = await _taskRepo.GetTasksForDoctorAsync(doctorId, cancellationToken);
        return tasks.Select(t => new FollowUpTaskDto
            {
            Id = t.Id,
            Description = t.TaskType.ToString(),
            Payload = t.Payload,
            ScheduledAt = t.ScheduledAt,
            Status = t.Status,
            PatientId = t.FollowUpRecord?.PatientId ?? 0
            }).ToList();
        }

    public async Task<IEnumerable<FollowUpTaskDto>> GetTasksForPatientAsync(long patientId, CancellationToken cancellationToken = default)
        {
        var tasks = await _taskRepo.GetTasksForPatientAsync(patientId, cancellationToken);
        return tasks.Select(t => new FollowUpTaskDto
            {
            Id = t.Id,
            Description = t.TaskType.ToString(),
            Payload = t.Payload,
            ScheduledAt = t.ScheduledAt,
            Status = t.Status
            }).ToList();
        }

    public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(DateTimeOffset from, CancellationToken cancellationToken = default)
        => await _appointmentRepo.GetUpcomingAppointmentsAsync(from, cancellationToken);
    }

