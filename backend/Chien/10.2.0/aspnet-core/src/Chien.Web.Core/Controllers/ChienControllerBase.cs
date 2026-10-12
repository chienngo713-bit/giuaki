using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace Chien.Controllers
{
    public abstract class ChienControllerBase : AbpController
    {
        protected ChienControllerBase()
        {
            LocalizationSourceName = ChienConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
