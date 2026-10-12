using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace Chien.EntityFrameworkCore;

public static class ChienDbContextConfigurer
{
    public static void Configure(DbContextOptionsBuilder<ChienDbContext> builder, string connectionString)
    {
        builder.UseSqlServer(connectionString);
    }

    public static void Configure(DbContextOptionsBuilder<ChienDbContext> builder, DbConnection connection)
    {
        builder.UseSqlServer(connection);
    }
}
