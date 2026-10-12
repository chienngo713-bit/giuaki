using Abp.AspNetCore;
using Abp.AspNetCore.TestBase;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Chien.EntityFrameworkCore;
using Chien.Web.Startup;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace Chien.Web.Tests;

[DependsOn(
    typeof(ChienWebMvcModule),
    typeof(AbpAspNetCoreTestBaseModule)
)]
public class ChienWebTestModule : AbpModule
{
    public ChienWebTestModule(ChienEntityFrameworkModule abpProjectNameEntityFrameworkModule)
    {
        abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
    }

    public override void PreInitialize()
    {
        Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(ChienWebTestModule).GetAssembly());
    }

    public override void PostInitialize()
    {
        IocManager.Resolve<ApplicationPartManager>()
            .AddApplicationPartsIfNotAddedBefore(typeof(ChienWebMvcModule).Assembly);
    }
}