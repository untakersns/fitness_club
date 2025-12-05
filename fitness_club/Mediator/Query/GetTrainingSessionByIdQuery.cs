using fitness_club.DTO;
using MediatR;

namespace fitness_club.Mediator.Query
{
    public class GetTrainingSessionByIdQuery: IRequest<TrainingSessionDto?>
    {
        public int Id { get; set; }
    }
}
