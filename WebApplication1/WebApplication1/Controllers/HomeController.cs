using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext db;

        public HomeController()
        {
            db = new AppDbContext();
        }

        public ActionResult Index()
        {
            var rooms = db.Rooms
                          .Include(r => r.RoomType)
                          .Where(r => r.Available == "Yes")
                          .Take(15)
                          .ToList();


            return View(rooms);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}