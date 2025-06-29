using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RestAPI.Dtos
{
    public class ConsultationMessageDto
    {
        public class CreateConsultationMessageDto
        {
            [Required]
            [StringLength(500, ErrorMessage = "Message cannot exceed 500 characters")]
            public string Message { get; set; } = string.Empty;

            [Required]
            public string ConsultationId { get; set; } = string.Empty;
        }

        public class UpdateConsultationMessageDto
        {
            [Required]
            [StringLength(500, ErrorMessage = "Message cannot exceed 500 characters")]
            public string Message { get; set; } = string.Empty;
        }
    }
}
