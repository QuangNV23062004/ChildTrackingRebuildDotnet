using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestAPI.Enums;
using RestAPI.Helpers;
using RestAPI.Models;

namespace RestAPI.Services.interfaces
{
    public interface IRequestService
    {
        Task<RequestModel> CreateRequest(RequestModel request, UserInfo requesterInfo);
        Task<RequestModel> UpdateRequest(string id, RequestModel request, UserInfo requesterInfo);
        Task<RequestModel> GetRequestById(string id, UserInfo requesterInfo);
        Task<PaginationResult<RequestModel>> GetAllRequests(
            QueryParams query,
            UserInfo requesterInfo,
            string? status
        );
        Task<RequestModel> DeleteRequest(string id, UserInfo requesterInfo);
        Task<PaginationResult<RequestModel>> GetRequestsByMemberId(
            string memberId,
            QueryParams query,
            UserInfo requesterInfo,
            string? status
        );
        Task<PaginationResult<RequestModel>> GetRequestsByDoctorId(
            string doctorId,
            QueryParams query,
            UserInfo requesterInfo
        );
        Task<RequestModel> UpdateRequestStatus(
            string id,
            RequestStatusEnum status,
            UserInfo requesterInfo
        );
    }
}
