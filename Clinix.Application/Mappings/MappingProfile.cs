using AutoMapper;
using Clinix.Application.Dto;
using Clinix.Domain.Entities.Appointments;
using Clinix.Domain.Entities.ApplicationUsers;

namespace Clinix.Application.Mapping;

public class MappingProfile : Profile
    {
    public MappingProfile()
        {
        CreateMap<Doctor, DoctorDto>();
        CreateMap<AppointmentSlot, SlotDto>();
        CreateMap<Appointment, AppointmentSummaryDto>()
            .ForMember(d => d.Slot, o => o.MapFrom(s => s.AppointmentSlot))
            .ForMember(d => d.Doctor, o => o.MapFrom(s => s.Doctor))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));
        }
    }
