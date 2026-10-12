using Abp.AspNetCore.Mvc.Authorization;
using Chien.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Chien.Web.Controllers;

public sealed class ToeicController : ChienControllerBase
{
    [HttpGet]
    public ActionResult Index()
    {
        ViewBag.Title = "TOEIC Practice";
        return View();
    }
}
