using Doyen.API.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Doyen.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpertsController : ControllerBase
    {
        // TODO: No content / not found status code
        // TODO: Add pagination
        [HttpGet("search")]
        [ProducesDefaultResponseType]
        [ProducesResponseType(typeof(List<Expert>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<List<Expert>> GetExpertsSearch([FromQuery] string keywords, [FromQuery] int limit = 50, [FromQuery] int offset = 0)
        {
            if (!AreLimitAndOffsetValid(limit, offset))
            {
                return BadRequest();
            }
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
            return new JsonResult(
        results,
        new JsonSerializerOptions { PropertyNamingPolicy = null });
        }

        [HttpGet("{identifier}")]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<ExpertDetails> GetExpertById([FromRoute] string identifier)
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
        public ActionResult<List<Expert>> GetCollaboratorsForExpertById([FromRoute] string identifier, [FromQuery] int limit = 50, [FromQuery] int offset = 0)
        {
            if (!AreLimitAndOffsetValid(limit, offset))
            {
                return BadRequest();
            }

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

        private bool AreLimitAndOffsetValid(int limit, int offset)
        {
            if (limit <= 0 || offset < 0)
            {
                return false;
            }
            return true;
        }
    }
}
