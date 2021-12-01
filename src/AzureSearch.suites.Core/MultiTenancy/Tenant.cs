using Abp.MultiTenancy;
using AzureSearch.suites.Authorization.Users;

namespace AzureSearch.suites.MultiTenancy
{
    public class Tenant : AbpTenant<User>
    {
        public Tenant()
        {            
        }

        public Tenant(string tenancyName, string name)
            : base(tenancyName, name)
        {
        }
    }
}
