using Abp.AspNetCore.Mvc.Authorization;
using Chien.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Chien.Web.Controllers;

[AbpMvcAuthorize]
public class HomeController : ChienControllerBase
{
    public ActionResult Index()
    {
        return View();
    }
}
