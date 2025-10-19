using AutoMapper;
using Clinix.Application.Dtos.FollowUp;
using Clinix.Domain.Entities.FollowUp;

namespace Clinix.Application.Mappings;

public class FollowUpMappingProfile : Profile
    {
    public FollowUpMappingProfile()
        {
        CreateMap<FollowUpRecord, FollowUpDto>()
            .ForMember(dst => dst.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<FollowUpPrescriptionSnapshot, FollowUpPrescriptionSnapshotDto>();
        }
    }

