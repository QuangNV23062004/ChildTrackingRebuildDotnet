using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RestAPI.Enums;

namespace RestAPI.Dtos
{
    public class ChildDto
    {
        public class CreateChildDto
        {
            [Required]
            [StringLength(100, MinimumLength = 1, ErrorMessage = "Name is required and must be between 1 and 100 characters")]
            public string Name { get; set; } = null!;

            [Required]
            [Range(0, 1, ErrorMessage = "Gender must be 0 (Boy) or 1 (Girl)")]
            public int Gender { get; set; }

            [Required]
            public DateTime BirthDate { get; set; }

            [StringLength(500, ErrorMessage = "Note cannot exceed 500 characters")]
            public string Note { get; set; } = "N/A";

            [Required]
            [RegularExpression("^(Parent|Guardian|Sibling|Other)$", ErrorMessage = "Relationship must be one of: Parent, Guardian, Sibling, Other")]
            public string Relationship { get; set; } = null!;

            [Required]
            public FeedingTypeEnum FeedingType { get; set; }

            [Required]
            [MinLength(1, ErrorMessage = "At least one allergy must be specified")]
            public List<AllergyEnum> Allergies { get; set; } = new();
        }

        public class ChildResponseDto
        {
            public string Id { get; set; } = null!;
            public string Name { get; set; } = null!;
            public DateTime BirthDate { get; set; }
            public string Note { get; set; } = null!;
            public int Gender { get; set; }
            public FeedingTypeEnum FeedingType { get; set; }
            public List<AllergyEnum> Allergies { get; set; } = new();
            public string GuardianId { get; set; } = null!;
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }

        public class UpdateChildDto
        {
            [Required]
            [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters")]
            public string Name { get; set; } = null!;

            [Required]
            [Range(0, 1, ErrorMessage = "Gender must be 0 (Boy) or 1 (Girl)")]
            public int Gender { get; set; }

            [Required]
            public DateTime BirthDate { get; set; }

            [Required]
            [StringLength(500, ErrorMessage = "Note cannot exceed 500 characters")]
            public string Note { get; set; } = null!;

            [Required]
            [RegularExpression("^(Parent|Guardian|Sibling|Other)$", ErrorMessage = "Relationship must be one of: Parent, Guardian, Sibling, Other")]
            public string Relationship { get; set; } = null!;

            [Required]
            public FeedingTypeEnum FeedingType { get; set; }

            [Required]
            [MinLength(1, ErrorMessage = "At least one allergy must be specified")]
            public List<AllergyEnum> Allergies { get; set; } = new();
        }
    }
}