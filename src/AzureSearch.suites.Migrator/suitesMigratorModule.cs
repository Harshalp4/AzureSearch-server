using Microsoft.Extensions.Configuration;
using Castle.MicroKernel.Registration;
using Abp.Events.Bus;
using Abp.Modules;
using Abp.Reflection.Extensions;
using AzureSearch.suites.Configuration;
using AzureSearch.suites.EntityFrameworkCore;
using AzureSearch.suites.Migrator.DependencyInjection;

namespace AzureSearch.suites.Migrator
{
    [DependsOn(typeof(suitesEntityFrameworkModule))]
    public class suitesMigratorModule : AbpModule
    {
        private readonly IConfigurationRoot _appConfiguration;

        public suitesMigratorModule(suitesEntityFrameworkModule abpProjectNameEntityFrameworkModule)
        {
            abpProjectNameEntityFrameworkModule.SkipDbSeed = true;

            _appConfiguration = AppConfigurations.Get(
                typeof(suitesMigratorModule).GetAssembly().GetDirectoryPathOrNull()
            );
        }

        public override void PreInitialize()
        {
            Configuration.DefaultNameOrConnectionString = _appConfiguration.GetConnectionString(
                suitesConsts.ConnectionStringName
            );

            Configuration.BackgroundJobs.IsJobExecutionEnabled = false;
            Configuration.ReplaceService(
                typeof(IEventBus), 
                () => IocManager.IocContainer.Register(
                    Component.For<IEventBus>().Instance(NullEventBus.Instance)
                )
            );

          
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(suitesMigratorModule).GetAssembly());
            ServiceCollectionRegistrar.Register(IocManager);
        }
    }
}
