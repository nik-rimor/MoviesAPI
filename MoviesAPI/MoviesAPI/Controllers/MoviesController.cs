using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.DTOs;
using MoviesAPI.Entities;
using MoviesAPI.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesAPI.Controllers
{
    [ApiController]
    [Route("api/movies")]
    public class MoviesController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IFileStorageService fileStorageService;
        private readonly string containerName = "movies";

        public MoviesController(ApplicationDbContext context,
                                IMapper mapper,
                                IFileStorageService fileStorageService)
        {
            this.context = context;
            this.mapper = mapper;
            this.fileStorageService = fileStorageService;
        }

        [HttpGet]
        public async Task<ActionResult<List<MovieDTO>>> Get()
        {
            var movieList = await context.Movies.ToListAsync();

            return mapper.Map<List<MovieDTO>>(movieList);
        }

        [HttpGet("{Id:int}", Name = "GetMovieById")]
        public async Task<ActionResult<MovieDTO>> Get(int Id)
        {
            var movieFromDb = await context.Movies.FirstOrDefaultAsync(m => m.Id == Id);
            if (movieFromDb == null) { return NotFound(); }

            return mapper.Map<MovieDTO>(movieFromDb);
        }

        [HttpPost]
        // Beacuse we wil receive an IFoormFile for the picture
        // we change [FromBody] binding to [FromForm]
        public async Task<ActionResult> Post([FromForm] MovieCreationDTO movieCreationDTO)
        {
            var movie = mapper.Map<Movie>(movieCreationDTO);

            if (movieCreationDTO.Poster != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await movieCreationDTO.Poster.CopyToAsync(memoryStream);
                    var content = memoryStream.ToArray();
                    var extension = Path.GetExtension(movieCreationDTO.Poster.FileName);
                    movie.Poster =
                        await fileStorageService.SaveFile(content, extension,
                        containerName, movieCreationDTO.Poster.ContentType);
                }
            }

            context.Add(movie);
            await context.SaveChangesAsync();

            var movieDTO = mapper.Map<MovieDTO>(movie);
            return new CreatedAtRouteResult("GetMovieById", new { movieDTO.Id }, movieDTO);
        }

        [HttpPut("{Id:int}")]
        public async Task<ActionResult> Put(int Id, [FromForm] MovieCreationDTO movieCreationDTO)
        {
            var movieFromDb = await context.Movies.FirstOrDefaultAsync(m => m.Id == Id);

            if (movieFromDb == null) { return NotFound(); }

            movieFromDb = mapper.Map(movieCreationDTO, movieFromDb);

            if (movieCreationDTO.Poster != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await movieCreationDTO.Poster.CopyToAsync(memoryStream);
                    var content = memoryStream.ToArray();
                    var extension = Path.GetExtension(movieCreationDTO.Poster.FileName);
                    movieFromDb.Poster =
                        await fileStorageService.EditFile(content, extension, containerName,
                                                            movieFromDb.Poster,
                                                            movieCreationDTO.Poster.ContentType);
                }
            }
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{Id:int}")]
        public async Task<ActionResult> Delete(int Id)
        {
            var movieFromDb = await context.Movies.FirstOrDefaultAsync(m => m.Id == Id);

            if (movieFromDb == null) { return NotFound(); }

            if (!string.IsNullOrWhiteSpace(movieFromDb.Poster))
            {
                await fileStorageService.DeleteFile(movieFromDb.Poster, containerName);
            }

            context.Remove(movieFromDb);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{Id:int}")]
        public async Task<ActionResult> Patch(int Id, [FromBody] JsonPatchDocument<MoviePatchDTO> patchDocument)
        {
            if (patchDocument == null) { return BadRequest(); }

            var movieFromDb = await context.Movies.FirstOrDefaultAsync(m => m.Id == Id);
            if (movieFromDb == null) { return NotFound(); }

            var movieDTO = mapper.Map<MoviePatchDTO>(movieFromDb);

            patchDocument.ApplyTo(movieDTO, ModelState);

            var isValid = TryValidateModel(movieDTO);
            if (!isValid) { return BadRequest(ModelState); }

            mapper.Map(movieDTO, movieFromDb);

            await context.SaveChangesAsync();

            return NoContent();
        }
    }
}
