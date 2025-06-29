using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestAPI.Helpers;
using RestAPI.Models;
using RestAPI.Repositories.Interfaces;

namespace RestAPI.Repositories.interfaces
{
    public interface IConsultationMessageRepository : IBaseRepository<ConsultationMessageModel>
    {
        Task<PaginationResult<ConsultationMessageModel>> GetConsultationMessages(
            string consultationId,
            QueryParams query
        );
    }
}
