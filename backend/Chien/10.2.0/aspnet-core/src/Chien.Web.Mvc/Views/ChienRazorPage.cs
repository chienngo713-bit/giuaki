using Abp.AspNetCore.Mvc.Views;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Mvc.Razor.Internal;

namespace Chien.Web.Views;

public abstract class ChienRazorPage<TModel> : AbpRazorPage<TModel>
{
    [RazorInject]
    public IAbpSession AbpSession { get; set; }

    protected ChienRazorPage()
    {
        LocalizationSourceName = ChienConsts.LocalizationSourceName;
    }
}
