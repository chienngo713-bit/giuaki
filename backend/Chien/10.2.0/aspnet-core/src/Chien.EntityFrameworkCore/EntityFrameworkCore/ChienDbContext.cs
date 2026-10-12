using Abp.Zero.EntityFrameworkCore;
using Chien.Authorization.Roles;
using Chien.Authorization.Users;
using Chien.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Chien.EntityFrameworkCore;

public class ChienDbContext : AbpZeroDbContext<Tenant, Role, User, ChienDbContext>
{
    /* Define a DbSet for each entity of the application */

    public ChienDbContext(DbContextOptions<ChienDbContext> options)
        : base(options)
    {
    }
}
