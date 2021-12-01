using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace AzureSearch.suites.Controllers
{
    public abstract class suitesControllerBase: AbpController
    {
        protected suitesControllerBase()
        {
            LocalizationSourceName = suitesConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
