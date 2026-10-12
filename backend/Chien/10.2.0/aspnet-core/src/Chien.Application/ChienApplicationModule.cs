using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Chien.Authorization;

namespace Chien;

[DependsOn(
    typeof(ChienCoreModule),
    typeof(AbpAutoMapperModule))]
public class ChienApplicationModule : AbpModule
{
    public override void PreInitialize()
    {
        Configuration.Authorization.Providers.Add<ChienAuthorizationProvider>();
    }

    public override void Initialize()
    {
        var thisAssembly = typeof(ChienApplicationModule).GetAssembly();

        IocManager.RegisterAssemblyByConvention(thisAssembly);

        Configuration.Modules.AbpAutoMapper().Configurators.Add(
            // Scan the assembly for classes which inherit from AutoMapper.Profile
            cfg => cfg.AddMaps(thisAssembly)
        );
    }
}
