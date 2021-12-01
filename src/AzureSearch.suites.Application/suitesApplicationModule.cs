using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using AzureSearch.suites.Authorization;

namespace AzureSearch.suites
{
    [DependsOn(
        typeof(suitesCoreModule), 
        typeof(AbpAutoMapperModule))]
    public class suitesApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Authorization.Providers.Add<suitesAuthorizationProvider>();
        }

        public override void Initialize()
        {
            var thisAssembly = typeof(suitesApplicationModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                // Scan the assembly for classes which inherit from AutoMapper.Profile
                cfg => cfg.AddMaps(thisAssembly)
            );
        }
    }
}
