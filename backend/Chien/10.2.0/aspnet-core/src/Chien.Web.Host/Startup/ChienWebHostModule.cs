using Abp.Modules;
using Abp.Reflection.Extensions;
using Chien.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Chien.Web.Host.Startup
{
    [DependsOn(
       typeof(ChienWebCoreModule))]
    public class ChienWebHostModule : AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public ChienWebHostModule(IWebHostEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(ChienWebHostModule).GetAssembly());
        }
    }
}
