﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoviesAPI.Helpers;
using MoviesAPI.Validations;
using System.Collections.Generic;

namespace MoviesAPI.DTOs
{
    public class MovieCreationDTO : MoviePatchDTO
    {

        [FileSizeValidator(4)]
        [ContentTypeValidator(ContentTypeGroup.Image)]
        public IFormFile Poster { get; set; }

        [ModelBinder(BinderType = typeof(TypeBinder<List<int>>))]
        public List<int> GenresIds { get; set; }

        [ModelBinder(BinderType = typeof(TypeBinder<List<ActorDTO>>))]
        public List<ActorDTO> Actors { get; set; }


    }
}
