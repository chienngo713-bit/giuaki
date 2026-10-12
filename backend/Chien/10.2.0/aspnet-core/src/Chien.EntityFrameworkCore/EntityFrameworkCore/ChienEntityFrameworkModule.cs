using Abp.EntityFrameworkCore.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Zero.EntityFrameworkCore;
using Chien.EntityFrameworkCore.Seed;
using Chien.EntityFrameworkCore.Toeic;

namespace Chien.EntityFrameworkCore;

[DependsOn(
    typeof(ChienCoreModule),
    typeof(AbpZeroCoreEntityFrameworkCoreModule))]
public class ChienEntityFrameworkModule : AbpModule
{
    /* Used it tests to skip dbcontext registration, in order to use in-memory database of EF Core */
    public bool SkipDbContextRegistration { get; set; }

    public bool SkipDbSeed { get; set; }

    public override void PreInitialize()
    {
        if (!SkipDbContextRegistration)
        {
            Configuration.Modules.AbpEfCore().AddDbContext<ChienDbContext>(options =>
            {
                if (options.ExistingConnection != null)
                {
                    ChienDbContextConfigurer.Configure(options.DbContextOptions, options.ExistingConnection);
                }
                else
                {
                    ChienDbContextConfigurer.Configure(options.DbContextOptions, options.ConnectionString);
                }
            });
        }
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(ChienEntityFrameworkModule).GetAssembly());
        IocManager.Register<ToeicDbContextFactory>(Abp.Dependency.DependencyLifeStyle.Singleton);
    }

    public override void PostInitialize()
    {
        if (!SkipDbSeed)
        {
            SeedHelper.SeedHostDb(IocManager);
        }
    }
}
