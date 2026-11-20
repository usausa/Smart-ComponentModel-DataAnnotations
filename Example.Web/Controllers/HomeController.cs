namespace Example.Web.Controllers;

using Example.Web.Models;

using Microsoft.AspNetCore.Mvc;

public sealed class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index() => View(new SampleForm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(SampleForm form)
    {
        ViewData["Valid"] = ModelState.IsValid;
        return View(form);
    }
}
