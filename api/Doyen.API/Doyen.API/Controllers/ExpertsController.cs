using Doyen.API.Dtos;
using Doyen.API.Elasticsearch;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Doyen.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpertsController : ControllerBase
    {
        // TODO: [FromQuery] string? luceneFilter = null,
        // TODO: No content / not found status code
        // TODO: Add pagination
        [HttpGet("search")]
        [ProducesDefaultResponseType]
        //[ProducesResponseType(typeof(List<Expert>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<List<Expert>> GetExpertsSearch([FromQuery] string keywords, [FromQuery] int limit = 50, [FromQuery] int offset = 0)
        {
            if (!AreLimitAndOffsetValid(limit, offset))
            {
                return BadRequest();
            }

            /*ElasticsearchRepository objSearch = new ElasticsearchRepository();
            var results = objSearch.GetPublicationsByKeywords(keywords);
            return new JsonResult(
            results,
            new JsonSerializerOptions { PropertyNamingPolicy = null });*/

            List<Expert> results = GetCovidExperts();
            return new JsonResult(
            results,
            new JsonSerializerOptions { PropertyNamingPolicy = null });


            /*
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
        new JsonSerializerOptions { PropertyNamingPolicy = null });*/
        }

        [HttpGet("{identifier}")]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<ExpertDetails> GetExpertById([FromRoute] string identifier)
        {
            var expert = CreateRandomExpert();
            return new ActionResult<ExpertDetails>(new ExpertDetails(expert.Name, expert.Identifier.ToString(), "Institution " + Guid.NewGuid().ToString()));
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

        private List<Expert> GetCovidExperts()
        {
            List<Expert> results = new List<Expert>()
            {
                new Expert("Conor", "Wall", "0000-0001-6674-692X"),
                new Expert("Li", "Zhang", "0000-0003-4263-7168"),
                new Expert("Yonghong", "Yu", null),
                new Expert("Akshi", "Kumar", null),
                new Expert("Rong", "Gao", null),

                new Expert("Santosh", "Kumar", null),
                new Expert("Mithilesh Kumar", "Chaube", null),
                new Expert("Saeed Hamood", "Alsamhi", null),
                new Expert("Sachin Kumar", "Gupta", null),
                new Expert("Mohsen", "Guizani", null),
                new Expert("Raffaele", "Gravina", null),
                new Expert("Giancarlo", "Fortino", null),

            };
            return results;
        }
        private Expert CreateRandomExpert()
        {
            return new Expert("Expert", Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
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
