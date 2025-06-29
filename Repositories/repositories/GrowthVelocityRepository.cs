using System;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RestAPI.Models;
using RestAPI.Repositories.database;
using RestAPI.Repositories.interfaces;

namespace RestAPI.Repositories.repositories;

public class GrowthVelocityRepository : Repository<GrowthVelocityModel>, IGrowthVelocitoryRepository
{
    public GrowthVelocityRepository(IOptions<MongoDBSettings> settings)
        : base(settings) { }

    public async Task<List<GrowthVelocityModel>> GetGrowthVelocityData(int gender)
    {
        try
        {
            var data = await _collection.Find(x => x.Gender == (GenderEnum)gender).ToListAsync();
            Console.WriteLine(
                "[GrowthVelocityRepository] Found "
                    + data.Count
                    + " growth velocity data for gender "
                    + gender
            );
            return data;
        }
        catch (System.Exception)
        {
            throw;
        }
    }
}
