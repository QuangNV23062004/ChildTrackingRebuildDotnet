using System;
using RestAPI.Models;
using RestAPI.Repositories.Interfaces;

namespace RestAPI.Repositories.interfaces;

public interface IWflhRepository : IBaseRepository<WflhModel>
{
    Task<List<WflhModel>> GetWflhData(double height, int gender);
}
