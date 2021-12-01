using Abp.AspNetCore;
using Abp.AspNetCore.TestBase;
using Abp.Modules;
using Abp.Reflection.Extensions;
using AzureSearch.suites.EntityFrameworkCore;
using AzureSearch.suites.Web.Startup;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace AzureSearch.suites.Web.Tests
{
    [DependsOn(
        typeof(suitesWebMvcModule),
        typeof(AbpAspNetCoreTestBaseModule)
    )]
    public class suitesWebTestModule : AbpModule
    {
        public suitesWebTestModule(suitesEntityFrameworkModule abpProjectNameEntityFrameworkModule)
        {
            abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
        } 
        
        public override void PreInitialize()
        {
            Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(suitesWebTestModule).GetAssembly());
        }
        
        public override void PostInitialize()
        {
            IocManager.Resolve<ApplicationPartManager>()
                .AddApplicationPartsIfNotAddedBefore(typeof(suitesWebMvcModule).Assembly);
        }
    }
}