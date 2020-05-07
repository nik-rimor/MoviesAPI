using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MoviesAPI.Entities;
using MoviesAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoviesAPI.Filters;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.DTOs;
using AutoMapper;

namespace MoviesAPI.Controllers
{
    [ApiController]
    [Route("api/genres")]
    public class GenresController : ControllerBase
    {
        private readonly ILogger<GenresController> _logger;
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public GenresController(ILogger<GenresController> logger,
                ApplicationDbContext context,
                IMapper mapper)
        {
            _logger = logger;
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<List<GenreDTO>>> Get()
        {
            // return await _genreRepo.GetAllGenres();
            var genres =  await context.Genres.AsNoTracking().ToListAsync();
            return mapper.Map<List<GenreDTO>>(genres);

        }

        [HttpGet("{Id:int}", Name = "GetGenreById")]
        public async Task<ActionResult<GenreDTO>> Get(int Id)
        {
            // var genre =  _genreRepo.GetGenreById(Id);
            var genre = await context.Genres.FirstOrDefaultAsync(x => x.Id == Id);
            if (genre == null)
            {
                return NotFound();
            }

            return mapper.Map<GenreDTO>(genre);
        }
        
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult> Post([FromBody] GenreCreationDTO genreCreation)
        {
            // map to Genre for database creation
            var genre = mapper.Map<Genre>(genreCreation);

            // _genreRepo.AddGenre(genre);
            context.Add(genre);
            await context.SaveChangesAsync();

            // map back to GenreDTO for returning crated genre
            var genreDTO = mapper.Map<GenreDTO>(genre);
            return new CreatedAtRouteResult("GetGenreById",
                new { genreDTO.Id }, genreDTO);
        }

        [HttpPut("{Id:int}")]
        public async Task<ActionResult> Put(int Id, [FromBody] GenreCreationDTO genreCreation)
        {
            var genre = mapper.Map<Genre>(genreCreation);

            var exists = await context.Genres.AnyAsync(x => x.Id == Id);
            if (!exists)
            {
                return NotFound();
            }
            genre.Id = Id;
            context.Entry(genre).State = EntityState.Modified;
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{Id:int}")]
        public async Task<ActionResult> Delete(int Id)
        {
            var genreFromDatabase = await context.Genres.FirstOrDefaultAsync(x => x.Id == Id);
            if (genreFromDatabase == null)
            {
                return NotFound();
            }

            context.Remove(genreFromDatabase);
            await context.SaveChangesAsync();

            return NoContent();
        }
    }
}
