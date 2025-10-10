using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Clinix.Application.Dtos;
using Clinix.Domain.Entities.Appointments;

namespace Clinix.Application.Interfaces.ServiceInterfaces;
public interface IAppointmentService
    {
    // Booking
    Task<ServiceResult<AppointmentDetailsDto>> BookAppointmentAsync(AppointmentCreateDto dto);

    // Rescheduling / updating
    Task<ServiceResult<AppointmentDetailsDto>> RescheduleAppointmentAsync(AppointmentUpdateDto dto);

    // Cancel appointment
    Task<ServiceResult<bool>> CancelAppointmentAsync(long appointmentId);

    // Doctor delays appointments (cascading)
    Task<ServiceResult<List<AppointmentListDto>>> DelayAppointmentsAsync(DelayAppointmentsDto dto);

    // Fetch available slots for a doctor
    Task<ServiceResult<List<AppointmentSlotDto>>> GetAvailableSlotsAsync(long doctorId, DateTime from, DateTime to);

    // List appointments for a doctor or patient
    Task<ServiceResult<List<AppointmentListDto>>> GetAppointmentsForUserAsync(long userId, bool isDoctor);

    // Get full appointment details
    Task<ServiceResult<AppointmentDetailsDto>> GetAppointmentDetailsAsync(long appointmentId);
    }
