using Clinix.Application.Dtos.Appointment;
using Clinix.Application.Interfaces.Functionalities;
using Clinix.Domain.Entities.Appointments;
using Clinix.Domain.Exceptions;
using FluentValidation;

namespace Clinix.Application.UseCases;

public class BookAppointmentUseCase
    {
    private readonly IAppointmentRepository _appointments;
    private readonly IDoctorScheduleRepository _schedules;
    private readonly INotificationService _notifications;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<CreateAppointmentRequest> _validator;

    public BookAppointmentUseCase(IAppointmentRepository appointments, IDoctorScheduleRepository schedules, INotificationService notifications, IUnitOfWork uow, IValidator<CreateAppointmentRequest> validator)
        {
        _appointments = appointments;
        _schedules = schedules;
        _notifications = notifications;
        _uow = uow;
        _validator = validator;
        }

    public async Task<Appointment> ExecuteAsync(CreateAppointmentRequest req, string requestedBy)
        {
        var validation = await _validator.ValidateAsync(req);
        if (!validation.IsValid) throw new ValidationException(validation.Errors);

        // Basic business checks
        var doctor = await _schedules.GetDoctorAsync(req.DoctorId) ?? throw new SchedulingException("Doctor not found");
        var workHours = await _schedules.GetDoctorWorkingHoursAsync(req.DoctorId) ?? throw new SchedulingException("Doctor working hours not configured");

        if (!workHours.IsWorkingOn(req.StartAt)) throw new SchedulingException("Doctor is not working at requested start time");
        if (!workHours.IsWorkingOn(req.EndAt)) throw new SchedulingException("Doctor is not working at requested end time");

        var overlap = (await _appointments.GetAppointmentsForDoctorInRangeAsync(req.DoctorId, req.StartAt, req.EndAt)).Any();
        if (overlap) throw new SchedulingException("Requested slot is not available");

        var appointment = new Appointment(req.DoctorId, req.PatientId, req.StartAt, req.EndAt, req.Reason);

        await _uow.BeginTransactionAsync();
        try
            {
            await _appointments.AddAsync(appointment);
            await _uow.CommitAsync();

            await _notifications.NotifyDoctorAsync(req.DoctorId, "New appointment requested", $"Appointment {appointment.Id} at {appointment.StartAt:o}");
            await _notifications.NotifyPatientAsync(req.PatientId, "Appointment requested", $"Your appointment request {appointment.Id} is pending approval by the doctor.");

            return appointment;
            }
        catch
            {
            await _uow.RollbackAsync();
            throw;
            }
        }
    }
