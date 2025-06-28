using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestAPI.Helpers;
using RestAPI.Models;
using RestAPI.Repositories.database;
using RestAPI.Repositories.interfaces;
using MongoDB.Driver;

namespace RestAPI.Repositories.repositories
{
    public class GrowthDataRepository : Repository<GrowthDataModel>, IGrowthDataRepository
    {
        public GrowthDataRepository(Microsoft.Extensions.Options.IOptions<MongoDBSettings> settings)
            : base(settings)
        {
        }

        public async Task<GrowthDataModel[]> GetAllGrowthDataByChildId(string id)
        {
            try
            {
                var data = await _collection.Find(x => x.ChildId == id && x.IsDeleted == false).ToListAsync();
                return data.ToArray();
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<PaginationResult<GrowthDataModel>> GetGrowthDataByChildId(string id, QueryParams query)
        {
            try
            {
                var skip = (query.Page - 1) * query.Size;
                var total = await _collection.CountDocumentsAsync(x => x.ChildId.ToString() == id);
                var data = await _collection.Find(x => x.ChildId.ToString() == id && x.IsDeleted == false).Skip(skip).Limit(query.Size).ToListAsync();
                return new PaginationResult<GrowthDataModel>{
                    Data = data.ToList(),
                    Page = query.Page,
                    Total = (int)total,
                    TotalPages = (int)Math.Ceiling(data.Count() / (double)query.Size)
                };
            }
            catch (System.Exception)
            {

                throw;
            }
        }

    }
}