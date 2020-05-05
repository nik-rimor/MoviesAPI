using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.DTOs;
using MoviesAPI.Entities;
using MoviesAPI.Helpers;
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
        public async Task<ActionResult<IndexMoviePageDTO>> Get()
        {
            var top = 6;
            var today = DateTime.Today;
            var upcomingReleases = await context.Movies
                .Where(x => x.ReleaseDate > today)
                .OrderBy(x => x.ReleaseDate)
                .Take(top)
                .ToListAsync();

            var inTheaters = await context.Movies
                .Where(x => x.InTheaters)
                .Take(top)
                .ToListAsync();

            var result = new IndexMoviePageDTO();
            result.InTheaters = mapper.Map<List<MovieDTO>>(inTheaters);
            result.UpcomingReleases = mapper.Map<List<MovieDTO>>(upcomingReleases);

            return result;
        }

        [HttpGet("filter")]
        public async Task<ActionResult<List<MovieDTO>>> Filter([FromQuery] FilterMoviesDTO filterMoviesDTO)
        {
            var moviesQueryable = context.Movies.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filterMoviesDTO.Title))
            {
                moviesQueryable = moviesQueryable.Where(x => x.Title.Contains(filterMoviesDTO.Title));
            }

            if (filterMoviesDTO.InTheaters)
            {
                moviesQueryable = moviesQueryable.Where(x => x.InTheaters);
            }
            
            if (filterMoviesDTO.UpcomingReleases)
            {
                var today = DateTime.Today;
                moviesQueryable = moviesQueryable.Where(x => x.ReleaseDate > today);
            }

            if (filterMoviesDTO.GenreId != 0) 
            {
                moviesQueryable = moviesQueryable
                    .Where(x => x.MoviesGenres.Select(y => y.GenreId)
                    .Contains(filterMoviesDTO.GenreId));
            }

            int totalAmountPages = await moviesQueryable.PaginationTotalPages(filterMoviesDTO.Pagination.RecordsPerPage);
            HttpContext.InsertPaginationParametersInResponse(totalAmountPages);
            var moviesList = await moviesQueryable.Paginate(filterMoviesDTO.Pagination, totalAmountPages).ToListAsync();

            return mapper.Map<List<MovieDTO>>(moviesList);

        }

        [HttpGet("{Id:int}", Name = "GetMovieById")]
        public async Task<ActionResult<MovieDetailsDTO>> Get(int Id)
        {
            var movieFromDb = await context.Movies
                .Include(x => x.MoviesActors).ThenInclude(a => a.Person)
                .Include(x => x.MoviesGenres).ThenInclude(g => g.Genre)
                .FirstOrDefaultAsync(m => m.Id == Id);

            if (movieFromDb == null) { return NotFound(); }

            return mapper.Map<MovieDetailsDTO>(movieFromDb);
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

            AnnotateActorsOrder(movie);

            context.Add(movie);
            await context.SaveChangesAsync();

            var movieDTO = mapper.Map<MovieDTO>(movie);
            return new CreatedAtRouteResult("GetMovieById", new { movieDTO.Id }, movieDTO);
        }


        private static void AnnotateActorsOrder(Movie movie)
        {
            if (movie.MoviesActors != null)
            {
                for (int i=0; i < movie.MoviesActors.Count; i++)
                {
                    movie.MoviesActors[i].Order = i;
                }
            }
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

            await context.Database.ExecuteSqlInterpolatedAsync($"delete from MoviesActors where MovieId = {movieFromDb.Id}; delete from MoviesGenres where MovieId = {movieFromDb.Id}");

            AnnotateActorsOrder(movieFromDb);
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
