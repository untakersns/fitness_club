using AutoMapper;
using fitness_club.Data;
using fitness_club.DTO;
using fitness_club.Mediator.Query;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace fitness_club.Mediator.Handler
{
    public class GetTrainingSessionsQueryHandler : IRequestHandler<GetTrainingSessionsQuery, PageResult<TrainingSessionDto>>
    {
        private readonly FCDbContext _context;
        private readonly IMapper _mapper;

        public GetTrainingSessionsQueryHandler(FCDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PageResult<TrainingSessionDto>> Handle(GetTrainingSessionsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.TrainingSessions
                .Include(ts => ts.Trainer)
                .AsQueryable();

            query = ApplyFilters(query, request.Query);

            query = ApplySorting(query, request.Query);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((request.Query.PageNumber - 1) * request.Query.PageSize)
                .Take(request.Query.PageSize)
                .ToListAsync(cancellationToken);

            var dtos = _mapper.Map<List<TrainingSessionDto>>(items);

            return new PageResult<TrainingSessionDto>
            {
                Items = dtos,
                PageNumber = request.Query.PageNumber,
                PageSize = request.Query.PageSize,
                TotalCount = totalCount
            };
        }

        private IQueryable<Entities.TrainingSession> ApplyFilters(IQueryable<Entities.TrainingSession> query, TrainingSessionQuery filters)
        {
            if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
            {
                var searchTerm = filters.SearchTerm.ToLower();
                query = query.Where(ts =>
                    ts.SessionName.ToLower().Contains(searchTerm) ||
                    ts.Trainer.FirstName.ToLower().Contains(searchTerm) ||
                    ts.Trainer.LastName.ToLower().Contains(searchTerm));
            }

            if (filters.TrainerId.HasValue)
            {
                query = query.Where(ts => ts.TrainerId == filters.TrainerId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filters.Specialization))
            {
                query = query.Where(ts => ts.Trainer.Specialization.ToLower().Contains(filters.Specialization.ToLower()));
            }

            if (filters.StartTimeFrom.HasValue)
            {
                query = query.Where(ts => ts.StartTime >= filters.StartTimeFrom.Value);
            }

            if (filters.StartTimeTo.HasValue)
            {
                query = query.Where(ts => ts.StartTime <= filters.StartTimeTo.Value);
            }

            if (filters.StartTimeHour.HasValue)
            {
                query = query.Where(ts => ts.StartTime.Hour == filters.StartTimeHour.Value);
            }

            if (filters.MinPrice.HasValue)
            {
                query = query.Where(ts => ts.Price >= filters.MinPrice.Value);
            }

            if (filters.MaxPrice.HasValue)
            {
                query = query.Where(ts => ts.Price <= filters.MaxPrice.Value);
            }

            if (!string.IsNullOrWhiteSpace(filters.SessionType))
            {
                query = query.Where(ts => ts.SessionType.ToLower().Contains(filters.SessionType.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(filters.DifficultyLevel))
            {
                query = query.Where(ts => ts.DifficultyLevel.ToLower().Contains(filters.DifficultyLevel.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(filters.FitnessGoal))
            {
                query = query.Where(ts => ts.FitnessGoal.ToLower().Contains(filters.FitnessGoal.ToLower()));
            }

            if (filters.MinTrainerRating.HasValue)
            {
                query = query.Where(ts => ts.Trainer.Rating >= filters.MinTrainerRating.Value);
            }

            if (filters.HasAvailableSpots.HasValue)
            {
                if (filters.HasAvailableSpots.Value)
                {
                    query = query.Where(ts => ts.CurrentParticipants < ts.MaxParticipants);
                }
                else
                {
                    query = query.Where(ts => ts.CurrentParticipants >= ts.MaxParticipants);
                }
            }

            if (!string.IsNullOrWhiteSpace(filters.Location))
            {
                query = query.Where(ts => ts.Location.ToLower().Contains(filters.Location.ToLower()));
            }

            if (filters.IsActive.HasValue)
            {
                query = query.Where(ts => ts.IsActive == filters.IsActive.Value);
            }

            return query;
        }

        private IQueryable<Entities.TrainingSession> ApplySorting(IQueryable<Entities.TrainingSession> query, TrainingSessionQuery filters)
        {
            var sortBy = filters.SortBy?.ToLower() ?? "starttime";
            var sortOrder = (filters.SortOrder?.ToLower() == "desc") ? true : false;

            switch (sortBy)
            {
                case "sessionname":
                    query = sortOrder ? query.OrderByDescending(ts => ts.SessionName) : query.OrderBy(ts => ts.SessionName);
                    break;
                case "price":
                    query = sortOrder ? query.OrderByDescending(ts => ts.Price) : query.OrderBy(ts => ts.Price);
                    break;
                case "rating":
                    query = sortOrder ? query.OrderByDescending(ts => ts.Trainer.Rating) : query.OrderBy(ts => ts.Trainer.Rating);
                    break;
                case "starttime":
                    query = sortOrder ? query.OrderByDescending(ts => ts.StartTime) : query.OrderBy(ts => ts.StartTime);
                    break;
                case "availablespots":
                    query = sortOrder ?
                        query.OrderByDescending(ts => ts.MaxParticipants - ts.CurrentParticipants) :
                        query.OrderBy(ts => ts.MaxParticipants - ts.CurrentParticipants);
                    break;
                case "duration":
                    query = sortOrder ?
                        query.OrderByDescending(ts => ts.EndTime - ts.StartTime) :
                        query.OrderBy(ts => ts.EndTime - ts.StartTime);
                    break;
                default:
                    query = sortOrder ? query.OrderByDescending(ts => ts.StartTime) : query.OrderBy(ts => ts.StartTime);
                    break;
            }

            return query;
        }
    }
}
