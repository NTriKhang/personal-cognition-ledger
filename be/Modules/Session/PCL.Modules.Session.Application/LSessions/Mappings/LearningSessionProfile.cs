using AutoMapper;
using PCL.Modules.Session.Application.LSessions.GetLearningSession;
using PCL.Modules.Session.Domain.LSessions;

namespace PCL.Modules.Session.Application.LSessions.Mappings
{
    public class LearningSessionProfile : Profile
    {
        public LearningSessionProfile()
        {
            CreateMap<LSession, LSessionDto>()
                .ForMember(dest => dest.LearningActivityIds, opt => opt.MapFrom(src => src.TaskIds));
        }
    }
}

