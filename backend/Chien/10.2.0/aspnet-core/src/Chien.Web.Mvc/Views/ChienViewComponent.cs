using Abp.AspNetCore.Mvc.ViewComponents;

namespace Chien.Web.Views;

public abstract class ChienViewComponent : AbpViewComponent
{
    protected ChienViewComponent()
    {
        LocalizationSourceName = ChienConsts.LocalizationSourceName;
    }
}
