using System;
using MongoDB.Driver;
using RestAPI.Models;
using RestAPI.Repositories.database;
using RestAPI.Repositories.interfaces;

namespace RestAPI.Repositories.repositories;

public class WflhRepository : Repository<WflhModel>, IWflhRepository
{
    public WflhRepository(Microsoft.Extensions.Options.IOptions<MongoDBSettings> settings)
            : base(settings)
    {
    }
    public async Task<List<WflhModel>> GetWflhData(double height, int gender)
    {

        try
        {
            var data = await _collection.Find(x => x.Height == height && x.Gender == (GenderEnum)gender).ToListAsync();
            Console.WriteLine("[WflhRepository] Found " + data.Count + " WFLH data for gender " + gender);
            return data;
        }
        catch (System.Exception)
        {

            throw;
        }


    }
}
