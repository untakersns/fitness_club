using AutoMapper;
using fitness_club.Data;
using fitness_club.DTO;
using fitness_club.Mediator.Query;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace fitness_club.Mediator.Handler
{
    public class GetTrainingSessionByIdQueryHandler : IRequestHandler<GetTrainingSessionByIdQuery, TrainingSessionDto?>
    {
        private readonly FCDbContext _context;
        private readonly IMapper _mapper;

        public GetTrainingSessionByIdQueryHandler(FCDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<TrainingSessionDto?> Handle(GetTrainingSessionByIdQuery request, CancellationToken cancellationToken)
        {
            var session = await _context.TrainingSessions
                .Include(ts => ts.Trainer)
                .FirstOrDefaultAsync(ts => ts.Id == request.Id, cancellationToken);

            if (session == null)
                return null;

            return _mapper.Map<TrainingSessionDto>(session);
        }
    }
}
