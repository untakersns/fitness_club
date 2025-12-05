using AutoMapper;
using fitness_club.Data;
using fitness_club.DTO;
using fitness_club.Mediator.Command;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace fitness_club.Mediator.Handler
{
    public class CreateTrainingSessionCommandHandler : IRequestHandler<CreateTrainingSessionCommand, TrainingSessionDto>
    {
        private readonly FCDbContext _context;
        private readonly IMapper _mapper;

        public CreateTrainingSessionCommandHandler(FCDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<TrainingSessionDto> Handle(CreateTrainingSessionCommand request, CancellationToken cancellationToken)
        {
            var session = _mapper.Map<Entities.TrainingSession>(request);

            _context.TrainingSessions.Add(session);
            await _context.SaveChangesAsync(cancellationToken);

            var sessionWithTrainer = await _context.TrainingSessions
                .Include(ts => ts.Trainer)
                .FirstAsync(ts => ts.Id == session.Id, cancellationToken);

            return _mapper.Map<TrainingSessionDto>(sessionWithTrainer);
        }
    }
}
