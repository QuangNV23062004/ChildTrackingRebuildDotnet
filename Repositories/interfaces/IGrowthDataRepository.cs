using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestAPI.Helpers;
using RestAPI.Models;
using RestAPI.Repositories.Interfaces;

namespace RestAPI.Repositories.interfaces
{
    public interface IGrowthDataRepository : IBaseRepository<GrowthDataModel>
    {
        Task<GrowthDataModel[]> GetAllGrowthDataByChildId(string id);
        Task<PaginationResult<GrowthDataModel>> GetGrowthDataByChildId(string id, QueryParams query);
    }
}