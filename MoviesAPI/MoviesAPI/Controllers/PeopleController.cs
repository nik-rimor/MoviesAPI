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
    [Route("api/people")]
    public class PeopleController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IFileStorageService fileStorageService;
        private readonly string containerName = "person";

        public PeopleController(ApplicationDbContext context, IMapper mapper,
                IFileStorageService fileStorageService)
        {
            this.context = context;
            this.mapper = mapper;
            this.fileStorageService = fileStorageService;
        }

        [HttpGet]
        public async Task<ActionResult<List<PersonDTO>>> Get([FromQuery] PaginationDTO pagination)
        {
            var queryable = context.Persons.AsQueryable();
            int totalAmountPages = await queryable.PaginationTotalPages(pagination.RecordsPerPage);
            HttpContext.InsertPaginationParametersInResponse(totalAmountPages);
            var personList = await queryable.Paginate(pagination, totalAmountPages).ToListAsync();

            return mapper.Map<List<PersonDTO>>(personList);
        }

        [HttpGet("{Id:int}", Name = "GetPersonById")]
        public async Task<ActionResult<PersonDTO>> Get(int Id)
        {
            var personFromDb = await context.Persons.AsNoTracking().FirstOrDefaultAsync(p => p.Id == Id);
            if (personFromDb == null)
            {
                return NotFound();
            }
            return mapper.Map<PersonDTO>(personFromDb);
        }

        [HttpPost]
        // Beacuse we wil receive an IFoormFile for the picture
        // we change [FromBody] binding to [FromForm]
        public async Task<ActionResult> Post([FromForm] PersonCreationDTO personCreation)        {
            
            var person = mapper.Map<Person>(personCreation);

            if (personCreation.Picture != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await personCreation.Picture.CopyToAsync(memoryStream);
                    var content = memoryStream.ToArray();
                    var extension = Path.GetExtension(personCreation.Picture.FileName);
                    person.Picture = 
                        await fileStorageService.SaveFile(content, extension, 
                        containerName, personCreation.Picture.ContentType);
                }
            }

            context.Add(person);
            await context.SaveChangesAsync();

            var personDTO = mapper.Map<PersonDTO>(person);
            return new CreatedAtRouteResult("GetPersonById", new { personDTO.Id }, personDTO);

        }

        [HttpPut("{Id:int}")]
        public async Task<ActionResult> Put(int Id, [FromForm] PersonCreationDTO personCreation)
        {
            var personFromDb = await context.Persons.FirstOrDefaultAsync(x => x.Id == Id);
            
            if (personFromDb == null) { return NotFound(); }

            personFromDb = mapper.Map(personCreation, personFromDb);

            if (personCreation.Picture != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await personCreation.Picture.CopyToAsync(memoryStream);
                    var content = memoryStream.ToArray();
                    var extension = Path.GetExtension(personCreation.Picture.FileName);
                    personFromDb.Picture =
                        await fileStorageService.EditFile(content, extension, containerName,
                                                            personFromDb.Picture,
                                                            personCreation.Picture.ContentType);
                }
            }
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{Id:int}")]
        public async Task<ActionResult> Delete(int Id)
        {
            var personFromDb = await context.Persons.FirstOrDefaultAsync(p => p.Id == Id);
            
            if (personFromDb == null) { return NotFound(); }

            if (!string.IsNullOrWhiteSpace(personFromDb.Picture))
            {
                await fileStorageService.DeleteFile(personFromDb.Picture, containerName);
            }

            context.Remove(personFromDb);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{Id:int}")]
        public async Task<ActionResult> Patch(int Id, [FromBody] JsonPatchDocument<PersonPatchDTO> patchDocument)
        {
            if (patchDocument == null) { return BadRequest(); }

            var personFromDb = await context.Persons.FirstOrDefaultAsync(p => p.Id == Id);
            if (personFromDb == null) { return NotFound(); }

            var personDTO = mapper.Map<PersonPatchDTO>(personFromDb);

            patchDocument.ApplyTo(personDTO, ModelState);

            var isValid = TryValidateModel(personDTO);
            if (!isValid) { return BadRequest(ModelState); }

            mapper.Map(personDTO, personFromDb);

            await context.SaveChangesAsync();

            return NoContent();
        } 
    }
}
