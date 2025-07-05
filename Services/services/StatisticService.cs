using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestAPI.Repositories.interfaces;
using RestAPI.Services.interfaces;

namespace RestAPI.Services.services
{
    public class StatisticService(
        IUserRepository userRepository,
        IRequestRepository requestRepository
    ) : IStatisticService
    {
        public async Task<List<CountData>> GetNewUsers(int value, string unit)
        {
            var users = await userRepository.GetNewUsers(value, unit);
            return users;
        }

        public async Task<List<CountData>> GetNewRequests(int value, string unit)
        {
            var requests = await requestRepository.GetNewRequests(value, unit);
            return requests;
        }
    }
}
