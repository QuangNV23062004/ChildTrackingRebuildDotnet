using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using RestAPI.Models;
using RestAPI.Repositories.database;
using RestAPI.Repositories.interfaces;

namespace RestAPI.Repositories.repositories
{
    public class GrowthMetricsForAgeRepository : Repository<GrowthMetricForAgeModel>, IGrowthMetricForAgeRepository
    {
        public GrowthMetricsForAgeRepository(Microsoft.Extensions.Options.IOptions<MongoDBSettings> settings)
            : base(settings)
        {
        }



        public async Task<List<GrowthMetricForAgeModel>> GetGrowthMetricsForAgeData(int gender, int age, string unit)
        {
            try
            {
                if (unit == "month")
                {
                    var data = await _collection.Find(x => x.Gender == (GenderEnum)gender && x.Age.InMonths == age).ToListAsync();
                    Console.WriteLine("[GrowthMetricsForAgeRepository] Found " + data.Count + " growth metrics for age data");
                    return data;
                }
                else if (unit == "day")
                {
                    var data = await _collection.Find(x => x.Gender == (GenderEnum)gender && x.Age.InDays == (double)age).ToListAsync();
                    Console.WriteLine("[GrowthMetricsForAgeRepository] Found " + data.Count + " growth metrics for age data");
                    return data;
                }
                else
                {
                    throw new ArgumentException("Invalid unit");
                }
            }
            catch (System.Exception)
            {

                throw;
            }
        }
    }
}