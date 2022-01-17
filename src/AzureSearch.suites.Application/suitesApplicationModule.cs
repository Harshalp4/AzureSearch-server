using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.EntityFrameworkCore;
using Abp.Modules;
using Abp.Reflection.Extensions;
using AzureSearch.suites.Authorization;
using AzureSearch.suites.Authorization.Roles;
using AzureSearch.suites.Authorization.Users;
using AzureSearch.suites.Roles.Dto;
using AzureSearch.suites.Users.Dto;
using Castle.MicroKernel.Registration;

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

            Configuration.Modules.AbpAutoMapper().Configurators.Add(cfg =>
            {
                // Role and permission
                cfg.CreateMap<Permission, string>().ConvertUsing(r => r.Name);
                cfg.CreateMap<RolePermissionSetting, string>().ConvertUsing(r => r.Name);

                cfg.CreateMap<CreateRoleDto, Role>().ForMember(x => x.Permissions, opt => opt.Ignore());
                cfg.CreateMap<RoleDto, Role>().ForMember(x => x.Permissions, opt => opt.Ignore());

                IRepository<Role, int> repository = IocManager.Resolve<IRepository<Role, int>>();
                // User and role
                //cfg.CreateMap<UserRole, string>().ConvertUsing((r) => {
                //    //TODO: Fix, this seems hacky
                //    Role role = repository.FirstOrDefault(r.RoleId);
                //    return role.DisplayName;
                //});

                IocManager.Release(repository);

                cfg.CreateMap<UserDto, User>();
                cfg.CreateMap<UserDto, User>().ForMember(x => x.Roles, opt => opt.Ignore());

                cfg.CreateMap<CreateUserDto, User>();
                cfg.CreateMap<CreateUserDto, User>().ForMember(x => x.Roles, opt => opt.Ignore());
            });
        }
    }
}
