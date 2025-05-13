using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private static List<Room> rooms = new List<Room>
    {
        new Room { Id = 1, Name = "Blue Origin Fams", Location = "Galle, Sri Lanka", PricePerNight = 50, ImageUrl = "/Images/blueorigin.jpg", Description = "A lovely house by the sea." },
        new Room { Id = 2, Name = "Ocean Land", Location = "Trincomalee, Sri Lanka", PricePerNight = 22, ImageUrl = "/Images/oceanland.jpg", Description = "Beachfront luxury resort." },
        // Thêm các phòng khác...
    };
        public ActionResult Index()
        {
           
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }
        // Trang chi tiết phòng
        public ActionResult RoomDetails(int id)
        {
            // Tạm thời trả về view trống với ID
            ViewBag.RoomId = id;
            return View();
        }

        [HttpPost]
        public ActionResult BookRoom(int roomId, DateTime checkIn, DateTime checkOut)
        {
            // Lưu thông tin đặt phòng
            // Redirect hoặc trả về view xác nhận
            return RedirectToAction("BookingConfirmation");
        }
    }

}
