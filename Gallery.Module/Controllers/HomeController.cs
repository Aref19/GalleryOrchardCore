
using Microsoft.AspNetCore.Mvc;


namespace Gallery.Module.Controllers;

public sealed class HomeController : Controller
{
 

    public ActionResult Index()
    {
      
        return View();
    }
}