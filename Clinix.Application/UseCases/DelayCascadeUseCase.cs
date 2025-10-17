using Clinix.Application.Dtos.Appointment;
using Clinix.Application.Interfaces.Functionalities;
using Clinix.Domain.Entities.Appointments;
using Clinix.Domain.Exceptions;

namespace Clinix.Application.UseCases;

/// <summary>
/// Performs a cascading delay across a doctor's upcoming appointments starting from a specific appointment.
/// Algorithm (simplified):
/// 1. Acquire schedule lock for doctor.
/// 2. Fetch all upcoming appointments for doctor from the appointment's start time.
/// 3. For each appointment in order, add the delay and ensure it's within working hours; if exceeds end-of-day, push to next working day at same start time shift preserved.
/// 4. Persist updates in a single transaction.
/// 5. Release lock and notify affected parties.
/// </summary>
public class DelayCascadeUseCase
    {
    private readonly IAppointmentRepository _appointments;
    private readonly IDoctorScheduleRepository _schedules;
    private readonly INotificationService _notifications;
    private readonly IUnitOfWork _uow;

    public DelayCascadeUseCase(IAppointmentRepository appointments, IDoctorScheduleRepository schedules, INotificationService notifications, IUnitOfWork uow)
        {
        _appointments = appointments;
        _schedules = schedules;
        _notifications = notifications;
        _uow = uow;
        }

    public async Task<IEnumerable<Appointment>> DelayCascadeAsync(DelayAppointmentRequest req)
        {
        if (req.DelayBy <= TimeSpan.Zero) throw new ArgumentException("Delay must be positive");

        // Acquire lock
        var locked = await _schedules.TryAcquireScheduleLockAsync(req.DoctorId, TimeSpan.FromSeconds(10));
        if (!locked) throw new SchedulingException("Could not acquire schedule lock; try again later");

        await _uow.BeginTransactionAsync();
        try
            {
            var baseAppt = await _appointments.GetByIdAsync(req.AppointmentId) ?? throw new SchedulingException("Appointment not found");
            var upcoming = (await _appointments.GetUpcomingAppointmentsForDoctorAsync(req.DoctorId, baseAppt.StartAt)).OrderBy(a => a.StartAt).ToList();

            var workingHours = await _schedules.GetDoctorWorkingHoursAsync(req.DoctorId) ?? throw new SchedulingException("Doctor working hours not configured");

            var modified = new List<Appointment>();

            // We'll shift each appointment by req.DelayBy, cascading
            foreach (var appt in upcoming)
                {
                var newStart = appt.StartAt + req.DelayBy;
                var newEnd = appt.EndAt + req.DelayBy;

                // If newEnd is outside working hours of that day, roll to next working day preserving duration and relative order
                if (!workingHours.IsWorkingOn(newStart) || !workingHours.IsWorkingOn(newEnd))
                    {
                    // Simple roll-forward: find next day where working hours accommodate the slot
                    var duration = appt.EndAt - appt.StartAt;
                    DateTimeOffset candidate = newStart;
                    bool placed = false;
                    for (int addDays = 0; addDays < 30; addDays++)
                        {
                        candidate = new DateTimeOffset(newStart.Date.AddDays(addDays + 1), newStart.Offset).Add(new TimeSpan(9, 0, 0)); // naive: start at 9am next day
                        var candidateEnd = candidate + duration;
                        if (workingHours.IsWorkingOn(candidate) && workingHours.IsWorkingOn(candidateEnd))
                            {
                            newStart = candidate;
                            newEnd = candidateEnd;
                            placed = true;
                            break;
                            }
                        }
                    if (!placed) throw new SchedulingException("Unable to place appointment after cascading delay within 30 days");
                    }

                appt.Reschedule(newStart, newEnd, req.RequestedBy.ToString(), $"Cascade delay {req.DelayBy}");
                modified.Add(appt);
                await _appointments.UpdateAsync(appt);
                }

            await _uow.CommitAsync();

            // Notifications (fire-and-forget)
            foreach (var m in modified)
                {
                await _notifications.NotifyPatientAsync(m.PatientId, "Appointment rescheduled", $"Your appointment {m.Id} has been moved to {m.StartAt:o}");
                }

            return modified;
            }
        catch
            {
            await _uow.RollbackAsync();
            throw;
            }
        finally
            {
            await _schedules.ReleaseScheduleLockAsync(req.DoctorId);
            }
        }
    }

