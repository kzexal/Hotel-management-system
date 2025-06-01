using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class DashBoardController : Controller
    {
        // GET: DashBoard
        public ActionResult UserDashBoard()
        {
            return View();
        }
    }
}
