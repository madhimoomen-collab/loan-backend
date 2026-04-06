using AutoMapper;
using Domain.DTOs;
using Domain.Models;

namespace Domain.Mappings;

public class LoanApplicationMappingProfile : Profile
{
    public LoanApplicationMappingProfile()
    {
        CreateMap<CreateLoanApplicationDto, LoanApplication>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => LoanApprovalStatus.Pending))
            .ForMember(dest => dest.DecisionDate, opt => opt.Ignore())
            .ForMember(dest => dest.DecisionReason, opt => opt.Ignore())
            .ForMember(dest => dest.Applicant, opt => opt.Ignore());

        CreateMap<LoanApplication, LoanApplicationDto>();
    }
}
