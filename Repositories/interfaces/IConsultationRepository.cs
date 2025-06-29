using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestAPI.Controllers;
using RestAPI.Helpers;
using RestAPI.Models;
using RestAPI.Repositories.Interfaces;

namespace RestAPI.Repositories.interfaces
{
    public interface IConsultationRepository : IBaseRepository<ConsultationModel>
    {
        Task<PaginationResult<ConsultationModel>> GetConsultationsByMemberId(
            string requestId,
            QueryParams query,
            string? status
        );

        Task<PaginationResult<ConsultationModel>> GetConsultationsByDoctorId(
            string requestId,
            QueryParams query,
            string? status
        );

        Task<PaginationResult<ConsultationModel>> GetConsultations(
            QueryParams query,
            string? status
        );

        Task<ConsultationModel> RateConsultationById(string id, int rating);
    }
}
