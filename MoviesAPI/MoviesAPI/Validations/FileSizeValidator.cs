using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesAPI.Validations
{
    public class FileSizeValidator : ValidationAttribute
    {
        private readonly int maxFileSizeInMBs;

        public FileSizeValidator(int MaxFileSizeInMBs = 1)
        {
            maxFileSizeInMBs = MaxFileSizeInMBs;
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

            if (formFile.Length > maxFileSizeInMBs * 1024 * 1024)
            {
                return new ValidationResult($"File size cannot be bigger than {maxFileSizeInMBs} MegaBytes");
            }

            return ValidationResult.Success;
        }
    }
}
