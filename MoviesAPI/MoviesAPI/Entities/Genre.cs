using MoviesAPI.Validations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesAPI.Entities
{
    public class Genre : IValidatableObject  //use for class level (model) velidation
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "The field with name {0} is required.")]
        [StringLength(100)]
        //[FirstLetterUppercase] this would be used for attrbute level validation
        public string Name { get; set; }


        // class level (model) validation
        // these validations run only after the attribute level validations
        // so they will not fire if an error has caused invalid state at the attrbute check
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(Name))
            {
                var firstLetter = Name[0].ToString();

                if(firstLetter != firstLetter.ToUpper())
                {
                    yield return new ValidationResult("First letter should be uppercase.",
                        new string[] { nameof(Name) }); 
                }
            }
        }
    }
}
