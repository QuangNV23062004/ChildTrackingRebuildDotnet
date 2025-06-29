using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestAPI.Dtos
{
    public class RequestDto
    {
        public class CreateRequestDto
        {
            public string ChildId { get; set; }
            public string DoctorId { get; set; }
            public string Message { get; set; }
        }

        public class UpdateRequestDto
        {
            public string ChildId { get; set; }
            public string DoctorId { get; set; }
            public string Message { get; set; }
        }

        public class UpdateRequestStatusDto
        {
            public string Status { get; set; }
        }
    }
}
