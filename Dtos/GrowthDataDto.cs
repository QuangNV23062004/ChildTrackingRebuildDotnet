using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestAPI.Dtos
{
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
            public DateTime InputDate { get; set; }
        }
    }
}
