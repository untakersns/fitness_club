using AutoMapper;
using fitness_club.DTO;
using fitness_club.Entities;
using fitness_club.Mediator.Command;

namespace fitness_club.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Trainer, TrainerDto>();
            CreateMap<TrainingSession, TrainingSessionDto>()
                .ForMember(dest => dest.Trainer, opt => opt.MapFrom(src => src.Trainer));
            CreateMap<CreateTrainingSessionCommand, TrainingSession>();
        }
    }
}