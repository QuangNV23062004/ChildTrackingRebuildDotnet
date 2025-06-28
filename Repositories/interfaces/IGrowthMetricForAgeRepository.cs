using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestAPI.Models;
using RestAPI.Repositories.Interfaces;

namespace RestAPI.Repositories.interfaces
{
    public interface IGrowthMetricForAgeRepository : IBaseRepository<GrowthMetricForAgeModel>
    {

        Task<List<GrowthMetricForAgeModel>> GetGrowthMetricsForAgeData(int age, int gender, string unit);
    }
}