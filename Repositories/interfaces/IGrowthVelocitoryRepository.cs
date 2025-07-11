using System;
using RestAPI.Models;
using RestAPI.Repositories.Interfaces;

namespace RestAPI.Repositories.interfaces;

public interface IGrowthVelocityRepository : IBaseRepository<GrowthVelocityModel>
{
    Task<List<GrowthVelocityModel>> GetGrowthVelocityData(int gender);
}
