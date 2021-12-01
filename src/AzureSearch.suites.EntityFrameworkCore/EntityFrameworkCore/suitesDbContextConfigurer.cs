using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace AzureSearch.suites.EntityFrameworkCore
{
    public static class suitesDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<suitesDbContext> builder, string connectionString)
        {
            builder.UseSqlServer(connectionString);
        }

        public static void Configure(DbContextOptionsBuilder<suitesDbContext> builder, DbConnection connection)
        {
            builder.UseSqlServer(connection);
        }
    }
}
