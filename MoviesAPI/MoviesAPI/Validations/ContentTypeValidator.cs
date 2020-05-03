using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesAPI.Validations
{
    public class ContentTypeValidator : ValidationAttribute
    {
        private readonly string[] validContentTypes;
        
        private readonly string[] imageContentTypes = new string[]
        {
            "image/jpeg",
            "image/png",
            "image/gif"
        };

        public ContentTypeValidator(string[] ValidContentTypes)
        {
            validContentTypes = ValidContentTypes;
        }

        public ContentTypeValidator(ContentTypeGroup contentTypeGroup)
        {
            switch (contentTypeGroup)
            {
                case ContentTypeGroup.Image:
                    validContentTypes = imageContentTypes;
                    break;
            }
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            IFormFile formFile = value as IFormFile;

            if (formFile == null)
            {
                return ValidationResult.Success;
            }

            if (!validContentTypes.Contains(formFile.ContentType))
            {
                return new ValidationResult($"The content type of {formFile.ContentType} is not recognized as image type. Please use : {string.Join(", ", validContentTypes)}");
            }

            return ValidationResult.Success;

        }
    }

    public enum ContentTypeGroup
    {
        Image
    }
}
