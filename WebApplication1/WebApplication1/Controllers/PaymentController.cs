using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using static WebApplication1.Controllers.RoomController;

namespace WebApplication1.Controllers
{
    public class PaymentController : Controller
    {
        // GET: Payment
        public ActionResult PaymentFailed(int roomId, string checkin, string checkout, string totalPrice)
        {
            var room = RoomData.GetRoomById(roomId); 
            ViewBag.Checkin = checkin;
            ViewBag.Checkout = checkout;
            ViewBag.TotalPrice = totalPrice;
            return View(room);
        }


        public ActionResult PaymentSuccess()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ProcessPayment(string CardNumber, string Bank, string ExpDate, string CVV, int roomId, string checkin, string checkout, string totalPrice)
        {
            if (string.IsNullOrWhiteSpace(CardNumber) || string.IsNullOrWhiteSpace(CVV)
                || !Regex.IsMatch(CardNumber, @"^\d+$") || !Regex.IsMatch(CVV, @"^\d+$"))
            {
                return RedirectToAction("PaymentFailed", new
                {
                    roomId = roomId,
                    checkin = checkin,
                    checkout = checkout,
                    totalPrice = totalPrice
                });
            }

            return RedirectToAction("PaymentSuccess");
        }

    }
}