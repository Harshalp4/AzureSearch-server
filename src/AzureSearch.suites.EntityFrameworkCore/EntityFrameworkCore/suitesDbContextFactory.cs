using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using AzureSearch.suites.Configuration;
using AzureSearch.suites.Web;

namespace AzureSearch.suites.EntityFrameworkCore
{
    /* This class is needed to run "dotnet ef ..." commands from command line on development. Not used anywhere else */
    public class suitesDbContextFactory : IDesignTimeDbContextFactory<suitesDbContext>
    {
        public suitesDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<suitesDbContext>();
            
            /*
             You can provide an environmentName parameter to the AppConfigurations.Get method. 
             In this case, AppConfigurations will try to read appsettings.{environmentName}.json.
             Use Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") method or from string[] args to get environment if necessary.
             https://docs.microsoft.com/en-us/ef/core/cli/dbcontext-creation?tabs=dotnet-core-cli#args
             */
            var configuration = AppConfigurations.Get(WebContentDirectoryFinder.CalculateContentRootFolder());

            suitesDbContextConfigurer.Configure(builder, configuration.GetConnectionString(suitesConsts.ConnectionStringName));

            return new suitesDbContext(builder.Options);
        }
    }
}
