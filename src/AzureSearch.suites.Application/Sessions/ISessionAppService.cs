using System.Threading.Tasks;
using Abp.Application.Services;
using AzureSearch.suites.Sessions.Dto;

namespace AzureSearch.suites.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
    }
}
