using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class RoomController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: /Room/Details/5
        public ActionResult Details(int id)
        {
            var room = db.Rooms.Include("RoomType").FirstOrDefault(r => r.RoomId == id);

            if (room == null)
                return HttpNotFound();

            return View(room);
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
