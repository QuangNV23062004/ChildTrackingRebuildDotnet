using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestAPI.Helpers;
using RestAPI.Models;

namespace RestAPI.Services.interfaces
{
    public interface IConsultationService
    {
        Task<PaginationResult<ConsultationModel>> GetConsultations(
            QueryParams query,
            string? status
        );

        Task<PaginationResult<ConsultationModel>> GetConsultationsByDoctorId(
            string requestId,
            QueryParams query,
            string? status
        );

        Task<PaginationResult<ConsultationModel>> GetConsultationsByMemberId(
            string requestId,
            QueryParams query,
            string? status
        );

        Task<ConsultationModel> RateConsultationById(string id, int rating, UserInfo requesterInfo);

        Task<ConsultationModel> UpdateConsultationStatus(
            string id,
            string status,
            UserInfo requesterInfo
        );

        Task<ConsultationModel> GetConsultationById(string id, UserInfo requesterInfo);
    }
}
