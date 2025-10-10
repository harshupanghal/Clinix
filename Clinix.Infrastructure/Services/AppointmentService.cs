using Clinix.Application.Dtos;
using Clinix.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Clinix.Application.Mappings;
using Clinix.Application.Interfaces.Functionalities;

public class AppointmentService : IAppointmentService
    {
    private readonly ClinixDbContext _dbContext;

    public AppointmentService(ClinixDbContext dbContext)
        {
        _dbContext = dbContext;
        }

    // -------------------- BOOK APPOINTMENT --------------------
    public async Task<ServiceResult<AppointmentDetailsDto>> BookAppointmentAsync(AppointmentCreateDto dto)
        {
        // 1. Check patient exists
        var patient = await _dbContext.Patients.Include(p => p.User)
            .FirstOrDefaultAsync(p => p.PatientId == dto.PatientId);
        if (patient == null) return new ServiceResult<AppointmentDetailsDto> { Success = false, Message = "Patient not found." };

        // 2. Check doctor exists
        var doctor = await _dbContext.Doctors.Include(d => d.User)
            .Include(d => d.Slots)
            .FirstOrDefaultAsync(d => d.DoctorId == dto.DoctorId);
        if (doctor == null) return new ServiceResult<AppointmentDetailsDto> { Success = false, Message = "Doctor not found." };
        if (!doctor.IsActive) return new ServiceResult<AppointmentDetailsDto> { Success = false, Message = "Doctor is inactive." };

        // 3. Check slot availability
        bool slotConflict = await _dbContext.AppointmentSlots
            .AnyAsync(s => s.DoctorId == dto.DoctorId &&
                           s.StartUtc < dto.EndTime &&
                           s.EndUtc > dto.StartTime &&
                           s.Status != SlotStatus.Available);
        if (slotConflict)
            return new ServiceResult<AppointmentDetailsDto> { Success = false, Message = "Requested time slot is not available." };

        // 4. Create appointment
        var appointment = dto.ToEntity();
        _dbContext.Appointments.Add(appointment);

        // 5. Optionally create a slot for this appointment if using dynamic slots
        var slot = new AppointmentSlot
            {
            DoctorId = dto.DoctorId,
            StartUtc = dto.StartTime,
            EndUtc = dto.EndTime,
            Status = SlotStatus.Booked
            };
        _dbContext.AppointmentSlots.Add(slot);

        appointment.AppointmentSlot = slot;
        await _dbContext.SaveChangesAsync();

        return new ServiceResult<AppointmentDetailsDto>
            {
            Success = true,
            Data = appointment.ToDetailsDto(patient.User.FullName, doctor.User.FullName)
            };
        }

    // -------------------- RESCHEDULE APPOINTMENT --------------------
    public async Task<ServiceResult<AppointmentDetailsDto>> RescheduleAppointmentAsync(AppointmentUpdateDto dto)
        {
        var appointment = await _dbContext.Appointments
            .Include(a => a.AppointmentSlot)
            .FirstOrDefaultAsync(a => a.Id == dto.AppointmentId);

        if (appointment == null)
            return new ServiceResult<AppointmentDetailsDto> { Success = false, Message = "Appointment not found." };

        // Check new slot availability
        bool slotConflict = await _dbContext.AppointmentSlots
            .AnyAsync(s => s.DoctorId == appointment.DoctorId &&
                           s.Id != appointment.AppointmentSlotId &&
                           s.StartUtc < dto.NewEndTime &&
                           s.EndUtc > dto.NewStartTime &&
                           s.Status != SlotStatus.Available);
        if (slotConflict)
            return new ServiceResult<AppointmentDetailsDto> { Success = false, Message = "New time slot is not available." };

        // Update appointment
        appointment.StartTime = dto.NewStartTime;
        appointment.EndTime = dto.NewEndTime;
        appointment.Reason = dto.Reason ?? appointment.Reason;
        appointment.Status = dto.Status ?? "Rescheduled";
        appointment.UpdatedAt = DateTime.UtcNow;

        // Update slot
        if (appointment.AppointmentSlot != null)
            {
            appointment.AppointmentSlot.StartUtc = dto.NewStartTime;
            appointment.AppointmentSlot.EndUtc = dto.NewEndTime;
            appointment.AppointmentSlot.Status = SlotStatus.Booked;
            }

        await _dbContext.SaveChangesAsync();

        var patient = await _dbContext.Patients.Include(p => p.User).FirstOrDefaultAsync(p => p.PatientId == appointment.PatientId);
        var doctor = await _dbContext.Doctors.Include(d => d.User).FirstOrDefaultAsync(d => d.DoctorId == appointment.DoctorId);

        return new ServiceResult<AppointmentDetailsDto>
            {
            Success = true,
            Data = appointment.ToDetailsDto(patient!.User.FullName, doctor!.User.FullName)
            };
        }

    // -------------------- CANCEL APPOINTMENT --------------------
    public async Task<ServiceResult<bool>> CancelAppointmentAsync(long appointmentId)
        {
        var appointment = await _dbContext.Appointments
            .Include(a => a.AppointmentSlot)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null)
            return new ServiceResult<bool> { Success = false, Message = "Appointment not found." };

        appointment.Status = "Cancelled";
        appointment.UpdatedAt = DateTime.UtcNow;

        if (appointment.AppointmentSlot != null)
            appointment.AppointmentSlot.Status = SlotStatus.Available;

        await _dbContext.SaveChangesAsync();

        return new ServiceResult<bool> { Success = true, Data = true };
        }

    // -------------------- DELAY APPOINTMENTS --------------------
    public async Task<ServiceResult<List<AppointmentListDto>>> DelayAppointmentsAsync(DelayAppointmentsDto dto)
        {
        var doctor = await _dbContext.Doctors.Include(d => d.Appointments)
            .FirstOrDefaultAsync(d => d.DoctorId == dto.DoctorId);

        if (doctor == null)
            return new ServiceResult<List<AppointmentListDto>> { Success = false, Message = "Doctor not found." };

        var upcomingAppointments = doctor.Appointments
            .Where(a => a.StartTime > DateTime.UtcNow && a.Status != "Cancelled")
            .OrderBy(a => a.StartTime)
            .ToList();

        var affectedList = new List<AppointmentListDto>();

        foreach (var appt in upcomingAppointments)
            {
            appt.StartTime = appt.StartTime.Add(dto.DelayDuration);
            appt.EndTime = appt.EndTime.Add(dto.DelayDuration);
            appt.Status = "Rescheduled";
            appt.UpdatedAt = DateTime.UtcNow;

            if (appt.AppointmentSlot != null)
                {
                appt.AppointmentSlot.StartUtc = appt.StartTime;
                appt.AppointmentSlot.EndUtc = appt.EndTime;
                appt.AppointmentSlot.Status = SlotStatus.Booked;
                }

            affectedList.Add(appt.ToListDto());
            }

        await _dbContext.SaveChangesAsync();

        return new ServiceResult<List<AppointmentListDto>> { Success = true, Data = affectedList };
        }

    // -------------------- GET AVAILABLE SLOTS --------------------
    public async Task<ServiceResult<List<AppointmentSlotDto>>> GetAvailableSlotsAsync(long doctorId, DateTime from, DateTime to)
        {
        var slots = await _dbContext.AppointmentSlots
            .Where(s => s.DoctorId == doctorId &&
                        s.StartUtc >= from &&
                        s.EndUtc <= to)
            .ToListAsync();

        var dtoList = slots.Select(s => s.ToSlotDto()).ToList();

        return new ServiceResult<List<AppointmentSlotDto>> { Success = true, Data = dtoList };
        }

    // -------------------- GET APPOINTMENTS FOR USER --------------------
    public async Task<ServiceResult<List<AppointmentListDto>>> GetAppointmentsForUserAsync(long userId, bool isDoctor)
        {
        List<Appointment> appointments;

        if (isDoctor)
            {
            appointments = await _dbContext.Appointments
                .Where(a => a.DoctorId == userId && a.Status != "Cancelled")
                .OrderBy(a => a.StartTime)
                .ToListAsync();
            }
        else
            {
            appointments = await _dbContext.Appointments
                .Where(a => a.PatientId == userId && a.Status != "Cancelled")
                .OrderBy(a => a.StartTime)
                .ToListAsync();
            }

        var dtoList = new List<AppointmentListDto>();
        foreach (var appt in appointments)
            {
            string? patientName = null;
            string? doctorName = null;

            if (!isDoctor)
                {
                var doctor = await _dbContext.Doctors.Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.DoctorId == appt.DoctorId);
                doctorName = doctor?.User.FullName;
                }
            else
                {
                var patient = await _dbContext.Patients.Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.PatientId == appt.PatientId);
                patientName = patient?.User.FullName;
                }

            dtoList.Add(appt.ToListDto(patientName, doctorName));
            }

        return new ServiceResult<List<AppointmentListDto>> { Success = true, Data = dtoList };
        }

    // -------------------- GET SINGLE APPOINTMENT DETAILS --------------------
    public async Task<ServiceResult<AppointmentDetailsDto>> GetAppointmentDetailsAsync(long appointmentId)
        {
        var appointment = await _dbContext.Appointments
            .Include(a => a.AppointmentSlot)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null)
            return new ServiceResult<AppointmentDetailsDto> { Success = false, Message = "Appointment not found." };

        var patient = await _dbContext.Patients.Include(p => p.User).FirstOrDefaultAsync(p => p.PatientId == appointment.PatientId);
        var doctor = await _dbContext.Doctors.Include(d => d.User).FirstOrDefaultAsync(d => d.DoctorId == appointment.DoctorId);

        return new ServiceResult<AppointmentDetailsDto>
            {
            Success = true,
            Data = appointment.ToDetailsDto(patient!.User.FullName, doctor!.User.FullName)
            };
        }
    }
