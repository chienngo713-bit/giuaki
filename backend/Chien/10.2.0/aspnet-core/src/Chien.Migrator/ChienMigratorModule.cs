using Abp.Events.Bus;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Chien.Configuration;
using Chien.EntityFrameworkCore;
using Chien.Migrator.DependencyInjection;
using Castle.MicroKernel.Registration;
using Microsoft.Extensions.Configuration;

namespace Chien.Migrator;

[DependsOn(typeof(ChienEntityFrameworkModule))]
public class ChienMigratorModule : AbpModule
{
    private readonly IConfigurationRoot _appConfiguration;

    public ChienMigratorModule(ChienEntityFrameworkModule abpProjectNameEntityFrameworkModule)
    {
        abpProjectNameEntityFrameworkModule.SkipDbSeed = true;

        _appConfiguration = AppConfigurations.Get(
            typeof(ChienMigratorModule).GetAssembly().GetDirectoryPathOrNull()
        );
    }

    public override void PreInitialize()
    {
        Configuration.DefaultNameOrConnectionString = _appConfiguration.GetConnectionString(
            ChienConsts.ConnectionStringName
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
        IocManager.RegisterAssemblyByConvention(typeof(ChienMigratorModule).GetAssembly());
        ServiceCollectionRegistrar.Register(IocManager);
    }
}
