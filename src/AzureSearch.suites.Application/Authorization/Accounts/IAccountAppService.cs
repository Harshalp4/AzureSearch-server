using System.Threading.Tasks;
using Abp.Application.Services;
using AzureSearch.suites.Authorization.Accounts.Dto;

namespace AzureSearch.suites.Authorization.Accounts
{
    public interface IAccountAppService : IApplicationService
    {
        Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

        Task<RegisterOutput> Register(RegisterInput input);
    }
}
