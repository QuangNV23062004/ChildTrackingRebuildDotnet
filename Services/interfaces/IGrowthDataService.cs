using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestAPI.Helpers;
using RestAPI.Models;
using RestAPI.Models.SubModels;

namespace RestAPI.Services.interfaces
{
    public interface IGrowthDataService
    {
        Task<(GrowthDataModel, List<GrowthVelocityResult>)> CreateGrowthDataAsync(
            UserInfo requesterInfo,
            string childId,
            GrowthDataModel growthData
        );

        Task<GrowthDataModel?> GetGrowthDataByIdAsync(string growthDataId, UserInfo requesterInfo);

        Task<PaginationResult<GrowthDataModel>> GetGrowthDataByChildIdAsync(
            string childId,
            UserInfo requesterInfo,
            QueryParams query
        );

        Task<bool> DeleteGrowthDataAsync(string growthDataId, UserInfo requesterInfo);

        Task<GrowthDataModel?> UpdateGrowthDataAsync(
            string growthDataId,
            UserInfo requesterInfo,
            GrowthDataModel updateData
        );

        Task<GrowthResult?> PublicGenerateGrowthDataAsync(
            GrowthDataModel growthData,
            DateTime birthDate,
            int gender
        );

        Task<List<GrowthVelocityResult>?> generateGrowthVelocityByChildId(
            UserInfo requesterInfo,
            string childId
        );
    }
}
