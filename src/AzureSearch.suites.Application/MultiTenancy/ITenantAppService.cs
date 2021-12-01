using Abp.Application.Services;
using AzureSearch.suites.MultiTenancy.Dto;

namespace AzureSearch.suites.MultiTenancy
{
    public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
    {
    }
}

