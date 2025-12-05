using fitness_club.DTO;
using MediatR;

namespace fitness_club.Mediator.Query
{
    public class GetTrainingSessionsQuery : IRequest<PageResult<TrainingSessionDto>>
    {
        public TrainingSessionQuery Query { get; set; } = new TrainingSessionQuery();
    }
}
