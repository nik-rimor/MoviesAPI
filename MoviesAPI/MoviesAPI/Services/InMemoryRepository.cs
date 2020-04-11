using Microsoft.Extensions.Logging;
using MoviesAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesAPI.Services
{
    public class InMemoryRepository : IGenreRepository
    {
        private readonly List<Genre> _genres;
        private readonly ILogger<InMemoryRepository> _logger;

        public InMemoryRepository(ILogger<InMemoryRepository> logger)
        {
            _logger = logger;
            _genres = new List<Genre>()
            {
                new Genre { Id = 1, Name = "Comedy" },
                new Genre { Id = 2, Name = "Action" }
            };
        }

        public async Task<List<Genre>> GetAllGenres()
        {
            _logger.LogInformation("Executing GetAllGenres");
            await Task.Delay(1);
            return _genres;
        }

        public Genre GetGenreById(int Id)
        {
            return _genres.FirstOrDefault(g => g.Id == Id);
        }

        public void AddGenre(Genre genre)
        {
            genre.Id = _genres.Max(x => x.Id) + 1;
            _genres.Add(genre);
        }
    }
}
