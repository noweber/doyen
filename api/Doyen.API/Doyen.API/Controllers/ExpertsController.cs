using Doyen.API.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Doyen.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpertsController : ControllerBase
    {
        // TODO: Add pagination
        [HttpGet("search")]
        [ProducesDefaultResponseType]
        [ProducesResponseType(typeof(List<Expert>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<List<Expert>> GetExpertsSearch([FromRoute] string keywords)
        {
            var results = new List<Expert>()
            {
                new Expert()
                {
                    Identifier = Guid.NewGuid(),
                    Name = "Dr. Doyen"
                },

             };

            for (int i = 0; i < 4; i++)
            {
                results.Add(CreateRandomExpert());
            }

            return new ActionResult<List<Expert>>(results);
        }

        [HttpGet("{identifier}")]
        [ProducesDefaultResponseType]
        [ProducesResponseType(typeof(ExpertDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<ExpertDetails> GetExpertById([FromRoute] Guid identifier)
        {
            var expert = CreateRandomExpert();
            return new ActionResult<ExpertDetails>(new ExpertDetails()
            {
                Identifier = expert.Identifier,
                Name = expert.Name,
                LastKnownInstitution = "Institution " + Guid.NewGuid().ToString()
            });
        }

        // TODO: Pagination
        [HttpGet("{identifier}/collaborators")]
        [ProducesDefaultResponseType]
        [ProducesResponseType(typeof(List<Expert>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<List<Expert>> GetCollaboratorsForExpertById([FromRoute] Guid identifier)
        {
            var results = new List<Expert>();

            for (int i = 0; i < 4; i++)
            {
                results.Add(CreateRandomExpert());
            }

            return new ActionResult<List<Expert>>(results);
        }

        private Expert CreateRandomExpert()
        {
            return new Expert()
            {
                Identifier = Guid.NewGuid(),
                Name = "Expert " + Guid.NewGuid().ToString()
            };
        }
    }
}
