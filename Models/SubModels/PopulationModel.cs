using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestAPI.Models.SubModels
{
    public class PopulationModel
    {
        public string LocalField { get; set; } = string.Empty;
        public string ForeignField { get; set; } = string.Empty;
        public string Collection { get; set; } = string.Empty;
        public string As { get; set; } = string.Empty;
    }
}
