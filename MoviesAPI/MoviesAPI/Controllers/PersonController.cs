using AutoMapper;
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
    [Route("api/person")]
    public class PersonController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IFileStorageService fileStorageService;
        private readonly string containerName = "person";

        public PersonController(ApplicationDbContext context, IMapper mapper,
                IFileStorageService fileStorageService)
        {
            this.context = context;
            this.mapper = mapper;
            this.fileStorageService = fileStorageService;
        }

        [HttpGet]
        public async Task<ActionResult<List<PersonDTO>>> Get()
        {
            var personList = await context.Persons.ToListAsync();

            return mapper.Map<List<PersonDTO>>(personList);
        }

        [HttpGet("{Id:int}", Name = "GetById")]
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
                        await fileStorageService.SaveFile(content, extension, containerName, personCreation.Picture.ContentType);
                }
            }

            context.Add(person);
            //await context.SaveChangesAsync();

            var personDTO = mapper.Map<PersonDTO>(person);
            return new CreatedAtRouteResult("GetById", new { personDTO.Id }, personDTO);

        }

    }
}
