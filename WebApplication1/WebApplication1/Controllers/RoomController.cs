using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class RoomController : Controller
    {
        public ActionResult Details(int id)
        {
            var rooms = new List<Room>
{
    new Room { Id = 1, Location = "Galle", PricePerNight = 50, PricePerDay = 200, Description = "Blue Origin Fams", Name = "Blue Origin Fams", ImageUrl = "https://c.animaapp.com/CYJfBrwE/img/rectangle-3@2x.png" },
    new Room { Id = 2, Location = "Trincomalee", PricePerNight = 22,PricePerDay = 100, Description = "Ocean Land", Name = "Ocean Land", ImageUrl = "https://c.animaapp.com/CYJfBrwE/img/rectangle-3@2x.png" },
    new Room { Id = 3, Location = "Dehiwala", PricePerNight = 856,PricePerDay = 1000, Description = "Stark House", Name = "Stark House", ImageUrl = "https://c.animaapp.com/CYJfBrwE/img/rectangle-3@2x.png" },
    new Room { Id = 4, Location = "Beruwala", PricePerNight = 62,PricePerDay = 250, Description = "Vinna Vill", Name = "Vinna Vill", ImageUrl = "https://c.animaapp.com/CYJfBrwE/img/rectangle-3@2x.png" },
    new Room { Id = 5, Location = "Kandy", PricePerNight = 72,PricePerDay = 350, Description = "Bobox", Name = "Bobox", ImageUrl = "https://c.animaapp.com/CYJfBrwE/img/rectangle-3@2x.png" }
};

            var room = rooms.FirstOrDefault(r => r.Id == id);
            if (room == null)
                return HttpNotFound();

            return View(room);
        }
    }
}
