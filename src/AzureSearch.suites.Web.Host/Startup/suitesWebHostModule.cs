using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using AzureSearch.suites.Configuration;

namespace AzureSearch.suites.Web.Host.Startup
{
    [DependsOn(
       typeof(suitesWebCoreModule))]
    public class suitesWebHostModule: AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public suitesWebHostModule(IWebHostEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(suitesWebHostModule).GetAssembly());
        }
    }
}
