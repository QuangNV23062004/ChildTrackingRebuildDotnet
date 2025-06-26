using System;
using RestAPI.Helpers;
using RestAPI.Models;
using RestAPI.Repositories.Interfaces;

namespace RestAPI.Repositories.interfaces;

public interface IChildRepository : IBaseRepository<ChildModel>
{
    Task<PaginationResult<ChildModel>> GetChildrenByUserId(string id, QueryParams query);
}
