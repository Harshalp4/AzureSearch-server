using Abp.Application.Features;
using Abp.Domain.Repositories;
using Abp.MultiTenancy;
using AzureSearch.suites.Authorization.Users;
using AzureSearch.suites.Editions;

namespace AzureSearch.suites.MultiTenancy
{
    public class TenantManager : AbpTenantManager<Tenant, User>
    {
        public TenantManager(
            IRepository<Tenant> tenantRepository, 
            IRepository<TenantFeatureSetting, long> tenantFeatureRepository, 
            EditionManager editionManager,
            IAbpZeroFeatureValueStore featureValueStore) 
            : base(
                tenantRepository, 
                tenantFeatureRepository, 
                editionManager,
                featureValueStore)
        {
        }
    }
}
