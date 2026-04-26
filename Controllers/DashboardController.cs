using Microsoft.AspNetCore.Mvc;

namespace OrdemServicoMvc.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
