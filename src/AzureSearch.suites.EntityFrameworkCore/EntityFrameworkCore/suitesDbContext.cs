using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using AzureSearch.suites.Authorization.Roles;
using AzureSearch.suites.Authorization.Users;
using AzureSearch.suites.MultiTenancy;

namespace AzureSearch.suites.EntityFrameworkCore
{
    public class suitesDbContext : AbpZeroDbContext<Tenant, Role, User, suitesDbContext>
    {
        /* Define a DbSet for each entity of the application */
        
        public suitesDbContext(DbContextOptions<suitesDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<sampleEntity> sampleEntitys { get; set; }
    }
}
