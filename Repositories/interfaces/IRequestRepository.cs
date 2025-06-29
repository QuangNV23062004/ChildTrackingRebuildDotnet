using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestAPI.Helpers;
using RestAPI.Models;
using RestAPI.Repositories.Interfaces;

namespace RestAPI.Repositories.interfaces
{
    public interface IRequestRepository : IBaseRepository<RequestModel>
    {
        Task<PaginationResult<RequestModel>> GetRequestsByMemberId(
            string memberId,
            QueryParams query,
            string? status
        );
        Task<PaginationResult<RequestModel>> GetRequestsByDoctorId(
            string doctorId,
            QueryParams query
        );
        Task<PaginationResult<RequestModel>> GetAllRequestWithPagination(
            QueryParams query,
            string? status
        );
    }
}
