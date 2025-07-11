using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestAPI.Dtos
{
    public class CurrentDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(
            object value,
            ValidationContext validationContext
        )
        {
            if (value == null)
                return ValidationResult.Success;

            DateTime inputDate = (DateTime)value;
            DateTime currentDate = DateTime.Today;

            if (inputDate > currentDate)
            {
                return new ValidationResult("Input date cannot be more than current date");
            }

            return ValidationResult.Success;
        }
    }

    public class GrowthDataDto
    {
        public class CreateGrowthDataDto
        {
            [Required]
            [Range(0.1, 300, ErrorMessage = "Height must be between 0.1 and 300 cm")]
            public double Height { get; set; }

            [Required]
            [Range(0.1, 200, ErrorMessage = "Weight must be between 0.1 and 200 kg")]
            public double Weight { get; set; }

            public double HeadCircumference { get; set; }

            public double ArmCircumference { get; set; }

            [Required]
            [CurrentDate(ErrorMessage = "Input date cannot be more than current date")]
            public DateTime InputDate { get; set; }
        }

        public class UpdateGrowthDataDto
        {
            [Required]
            [Range(0.1, 300, ErrorMessage = "Height must be between 0.1 and 300 cm")]
            public double Height { get; set; }

            [Required]
            [Range(0.1, 200, ErrorMessage = "Weight must be between 0.1 and 200 kg")]
            public double Weight { get; set; }

            public double HeadCircumference { get; set; }

            public double ArmCircumference { get; set; }

            [Required]
            [CurrentDate(ErrorMessage = "Input date cannot be more than current date")]
            public DateTime InputDate { get; set; }
        }

        public class PublicCreateGrowthDataDto
        {
            [Required]
            public DateTime BirthDate { get; set; }

            [Required]
            public int Gender { get; set; }

            [Required]
            [Range(0.1, 300, ErrorMessage = "Height must be between 0.1 and 300 cm")]
            public double Height { get; set; }

            [Required]
            [Range(0.1, 200, ErrorMessage = "Weight must be between 0.1 and 200 kg")]
            public double Weight { get; set; }

            public double HeadCircumference { get; set; }

            public double ArmCircumference { get; set; }

            [Required]
            [CurrentDate(ErrorMessage = "Input date cannot be more than current date")]
            public DateTime InputDate { get; set; }
        }
    }
}
