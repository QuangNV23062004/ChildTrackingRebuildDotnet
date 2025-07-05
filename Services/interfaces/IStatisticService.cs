using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestAPI.Services.interfaces
{
    public record CountData(DateTime Date, int Count);

    public interface IStatisticService
    {
        Task<List<CountData>> GetNewUsers(int value, string unit);
        Task<List<CountData>> GetNewRequests(int value, string unit);
    }
}
