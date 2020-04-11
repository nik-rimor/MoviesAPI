using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MoviesAPI.Entities;
using MoviesAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesAPI.Controllers
{
    [ApiController]
    [Route("api/genres")]
    public class GenresController : ControllerBase
    {
        private readonly IGenreRepository _genreRepo;
        private readonly ILogger<GenresController> _logger;
        public GenresController(IGenreRepository genreRepo,
                ILogger<GenresController> logger)
        {
            _genreRepo = genreRepo;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<Genre>>> Get()
        {
            _logger.LogInformation("Getting all the genres");
            return await _genreRepo.GetAllGenres();
        }

        [HttpGet("{Id:int}", Name = "GetGenreById")]
        public ActionResult<Genre> Get(int Id)
        {
            _logger.LogDebug("GetById method is executing ...");
            var genre =  _genreRepo.GetGenreById(Id);
            if (genre == null)
            {
                _logger.LogWarning($"Genre with Id {Id} not found");
                return NotFound();
            }

            return genre;

        }
        
        [HttpPost]
        public ActionResult Post([FromBody] Genre genre)
        {
            _genreRepo.AddGenre(genre);
            return new CreatedAtRouteResult("GetGenreById",
                new { Id = genre.Id }, genre);
        }

        [HttpPut]
        public ActionResult Put([FromBody] Genre genre)
        {
            return NoContent();
        }

        [HttpDelete]
        public ActionResult Delete()
        {
            return NoContent();
        }
    }
}
