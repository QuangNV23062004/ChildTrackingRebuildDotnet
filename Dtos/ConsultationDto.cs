using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestAPI.Dtos
{
    public class ConsultationDto
    {
        public class UpdateConsultationStatusDto
        {
            public string Status { get; set; }
        }

        public class RateConsultationDto
        {
            public int Rating { get; set; }
        }
    }
}
