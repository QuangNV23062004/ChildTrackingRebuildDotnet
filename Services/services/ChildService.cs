using System;
using MongoDB.Bson;
using RestAPI.Enums;
using RestAPI.Helpers;
using RestAPI.Models;
using RestAPI.Models.SubModels;
using RestAPI.Repositories.interfaces;
using RestAPI.Repositories.repositories;
using RestAPI.Services.interfaces;

namespace RestAPI.Services.services;

public class ChildService(IChildRepository _childRepository, IUserRepository _userRepository)
    : IChildService
{
    public async Task<ChildModel> CreateChildAsync(UserInfo requesterInfo, ChildModel childData)
    {
        try
        {
            if (childData == null)
            {
                throw new ArgumentNullException(nameof(childData), "Child data cannot be null");
            }
            if (requesterInfo == null)
            {
                throw new ArgumentNullException(
                    nameof(requesterInfo),
                    "Requester info cannot be null"
                );
            }
            childData.Id = ObjectId.GenerateNewId().ToString();
            childData.GuardianId = requesterInfo.UserId;
            return await _childRepository.CreateAsync(childData);
        }
        catch (System.Exception)
        {
            throw;
        }
    }

    public async Task<ChildModel?> GetChildByIdAsync(string childId, UserInfo requesterInfo)
    {
        try
        {
            var child = await _childRepository.GetByIdAsync(
                childId,
                new PopulationModel[]
                {
                    new PopulationModel
                    {
                        LocalField = "guardianId",
                        ForeignField = "_id",
                        Collection = "users",
                        As = "guardian",
                    },
                }
            );
            if (child == null)
            {
                throw new KeyNotFoundException("Child with the specified id does not exist");
            }

            return child;
        }
        catch (System.Exception)
        {
            throw;
        }
    }

    public async Task<PaginationResult<ChildModel>> GetChildrenByUserIdAsync(
        string userId,
        UserInfo requesterInfo,
        QueryParams query
    )
    {
        var checkUser = await _userRepository.GetByIdAsync(requesterInfo.UserId);
        if (checkUser == null)
        {
            throw new KeyNotFoundException("User with the specified id does not exist");
        }

        var isAdmin = checkUser.Role == RoleEnum.Admin.ToString();
        var isSelf = userId == requesterInfo.UserId;

        if (!isAdmin && !isSelf)
        {
            throw new UnauthorizedAccessException(
                "You do not have permission to access this resource"
            );
        }

        var children = await _childRepository.GetChildrenByUserId(userId, query);
        return children;
    }

    public async Task<ChildModel?> DeleteChildAsync(string childId, UserInfo requesterInfo)
    {
        try
        {
            var checkUser = await _userRepository.GetByIdAsync(requesterInfo.UserId);
            if (checkUser == null)
            {
                throw new KeyNotFoundException("User with the specified id does not exist");
            }

            var child = await _childRepository.GetByIdAsync(childId);
            if (child == null)
            {
                throw new KeyNotFoundException("Child with the specified id does not exist");
            }
            var isAdmin = checkUser.Role == RoleEnum.Admin.ToString();
            var isSelf = child.GuardianId.ToString() == requesterInfo.UserId;

            if (!isAdmin && !isSelf)
            {
                throw new UnauthorizedAccessException(
                    "You do not have permission to perform this action"
                );
            }

            var deletedChild = await _childRepository.DeleteAsync(childId);
            return deletedChild;
        }
        catch (System.Exception)
        {
            throw;
        }
    }

    public async Task<ChildModel?> UpdateChildAsync(
        string childId,
        UserInfo requesterInfo,
        ChildModel updateData
    )
    {
        try
        {
            var checkUser = await _userRepository.GetByIdAsync(requesterInfo.UserId);
            if (checkUser == null)
            {
                throw new KeyNotFoundException("User with the specified id does not exist");
            }

            var child = await _childRepository.GetByIdAsync(childId);
            if (child == null)
            {
                throw new KeyNotFoundException("Child with the specified id does not exist");
            }
            var isAdmin = checkUser.Role == RoleEnum.Admin.ToString();
            var isSelf = child.GuardianId.ToString() == requesterInfo.UserId;

            if (!isAdmin && !isSelf)
            {
                throw new UnauthorizedAccessException(
                    "You do not have permission to perform this action"
                );
            }
            if (updateData.Name != null)
            {
                child.Name = updateData.Name;
            }
            if (updateData.Gender != null)
            {
                child.Gender = updateData.Gender;
            }
            if (updateData.BirthDate != null)
            {
                child.BirthDate = updateData.BirthDate;
            }
            if (updateData.Note != null)
            {
                child.Note = updateData.Note;
            }
            if (updateData.FeedingType != null)
            {
                child.FeedingType = updateData.FeedingType;
            }
            if (updateData.Allergies != null)
            {
                child.Allergies = updateData.Allergies;
            }
            child.UpdatedAt = DateTime.UtcNow;
            var updatedChild = await _childRepository.UpdateAsync(childId, child);
            return updatedChild;
        }
        catch (System.Exception)
        {
            throw;
        }
    }
}
