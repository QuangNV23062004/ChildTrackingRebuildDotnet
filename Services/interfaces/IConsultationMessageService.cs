using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestAPI.Helpers;
using RestAPI.Models;

namespace RestAPI.Services.interfaces
{
    public interface IConsultationMessageService
    {
        Task<ConsultationMessageModel> CreateConsultationMessage(
            ConsultationMessageModel message,
            UserInfo userInfo
        );
        Task<PaginationResult<ConsultationMessageModel>> GetConsultationMessages(
            string consultationId,
            QueryParams query
        );

        Task<ConsultationMessageModel> GetConsultationMessageById(string id);

        Task<ConsultationMessageModel> UpdateConsultationMessage(
            string id,
            string message,
            UserInfo userInfo
        );

        Task<ConsultationMessageModel> DeleteConsultationMessage(string id, UserInfo userInfo);
    }
}
