using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class BookingController : Controller
    {
        public ActionResult BookingPage(int roomId)

        {
            // Danh sách tĩnh các phòng (tạm thời)
            var rooms = new List<Room>
            {
                new Room { Id = 1, Name = "Blue Origin Fams", Location = "Galle", PricePerDay = 200, ImageUrl = "https://c.animaapp.com/CYJfBrwE/img/rectangle-3@2x.png" },
                new Room { Id = 2, Name = "Ocean Land", Location = "Trincomalee", PricePerDay = 100, ImageUrl = "https://c.animaapp.com/CYJfBrwE/img/rectangle-3@2x.png" },
                new Room { Id = 3, Name = "Stark House", Location = "Dehiwala", PricePerDay = 1000, ImageUrl = "https://c.animaapp.com/CYJfBrwE/img/rectangle-3@2x.png" },
                new Room { Id = 4, Name = "Vinna Vill", Location = "Beruwala", PricePerDay = 250, ImageUrl = "https://c.animaapp.com/CYJfBrwE/img/rectangle-3@2x.png" },
                new Room { Id = 5, Name = "Bobox", Location = "Kandy", PricePerDay = 350, ImageUrl = "https://c.animaapp.com/CYJfBrwE/img/rectangle-3@2x.png" }
            };

            var room = rooms.FirstOrDefault(r => r.Id == roomId);
            if (room == null)
                return HttpNotFound();

            return View(room);
        }
        public ActionResult Payment(int roomId)

        {
          
            var rooms = new List<Room>
            {
                new Room { Id = 1, Name = "Blue Origin Fams", Location = "Galle", PricePerDay = 200, ImageUrl = "https://c.animaapp.com/CYJfBrwE/img/rectangle-3@2x.png" },
                new Room { Id = 2, Name = "Ocean Land", Location = "Trincomalee", PricePerDay = 100, ImageUrl = "https://c.animaapp.com/CYJfBrwE/img/rectangle-3@2x.png" },
                new Room { Id = 3, Name = "Stark House", Location = "Dehiwala", PricePerDay = 1000, ImageUrl = "https://c.animaapp.com/CYJfBrwE/img/rectangle-3@2x.png" },
                new Room { Id = 4, Name = "Vinna Vill", Location = "Beruwala", PricePerDay = 250, ImageUrl = "https://c.animaapp.com/CYJfBrwE/img/rectangle-3@2x.png" },
                new Room { Id = 5, Name = "Bobox", Location = "Kandy", PricePerDay = 350, ImageUrl = "https://c.animaapp.com/CYJfBrwE/img/rectangle-3@2x.png" }
            };

            var room = rooms.FirstOrDefault(r => r.Id == roomId);
            if (room == null)
                return HttpNotFound();

            return View(room);
        }
    }
}

