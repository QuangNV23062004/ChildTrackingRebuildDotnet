using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestAPI.Enums;
using RestAPI.Helpers;
using RestAPI.Models;
using RestAPI.Models.SubModels;
using RestAPI.Repositories.Interfaces;

namespace RestAPI.Repositories.interfaces
{
    public interface IBlogRepository : IBaseRepository<BlogModel>
    {
        Task<PaginationResult<BlogModel>> GetAllAsyncWithPagination(
            QueryParams query,
            PopulationModel[]? populationParams = null,
            BlogStatus? status = null
        );
    }
}
