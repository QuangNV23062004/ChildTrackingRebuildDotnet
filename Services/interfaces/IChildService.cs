using System;
using RestAPI.Models;
using RestAPI.Helpers;
namespace RestAPI.Services.interfaces;

public interface IChildService
{
    Task<ChildModel> CreateChildAsync(UserInfo requesterInfo, ChildModel childData);
    Task<ChildModel?> GetChildByIdAsync(string childId, UserInfo requesterInfo);
    Task<PaginationResult<ChildModel>> GetChildrenByUserIdAsync(string userId, UserInfo requesterInfo, QueryParams query);
    Task<ChildModel?> DeleteChildAsync(string childId, UserInfo requesterInfo);
    Task<ChildModel?> UpdateChildAsync(string childId, UserInfo requesterInfo, ChildModel updateData);
}
