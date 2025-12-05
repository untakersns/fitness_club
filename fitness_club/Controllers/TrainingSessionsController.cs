using fitness_club.DTO;
using fitness_club.Mediator.Command;
using fitness_club.Mediator.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace fitness_club.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrainingSessionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TrainingSessionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<PageResult<TrainingSessionDto>>> GetTrainingSessions([FromQuery] TrainingSessionQuery query)
        {
            var request = new GetTrainingSessionsQuery { Query = query };
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TrainingSessionDto>> GetTrainingSession(int id)
        {
            var request = new GetTrainingSessionByIdQuery { Id = id };
            var result = await _mediator.Send(request);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<TrainingSessionDto>> CreateTrainingSession([FromBody] CreateTrainingSessionCommand command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetTrainingSession), new { id = result.Id }, result);
        }
    }
}