using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace YourNamespace.Controllers
{
    public class BookingController : Controller
    {
        public IActionResult BookingPage(int roomId)
        {
            // Existing code
            return View();
        }

        [HttpPost]
        public IActionResult BookRoom(int roomId, DateTime checkInDate, DateTime checkOutDate)
        {
            if (!DateTime.TryParse(checkInDate.ToString("yyyy-MM-dd"), out DateTime parsedCheckInDate) ||
                !DateTime.TryParse(checkOutDate.ToString("yyyy-MM-dd"), out DateTime parsedCheckOutDate))
            {
                TempData["BookingError"] = "Ngày check-in hoặc check-out không hợp lệ.";
                return RedirectToAction("BookingPage", new { roomId });
            }

            if (parsedCheckInDate < DateTime.Today)
            {
                TempData["BookingError"] = "Ngày check-in không thể là ngày trong quá khứ.";
                return RedirectToAction("BookingPage", new { roomId });
            }

            if (parsedCheckOutDate <= parsedCheckInDate)
            {
                TempData["BookingError"] = "Ngày check-out phải sau ngày check-in.";
                return RedirectToAction("BookingPage", new { roomId });
            }

            var start = parsedCheckInDate;
            var end = parsedCheckOutDate;

            // Existing code
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Payment(FormCollection form)
        {
            try
            {
                if (!int.TryParse(form["RoomId"], out int roomId) ||
                    !int.TryParse(form["BookingAmount"], out int totalPrice) ||
                    !DateTime.TryParse(form["CheckInDate"], out DateTime checkInDate) ||
                    !DateTime.TryParse(form["CheckOutDate"], out DateTime checkOutDate))
                {
                    TempData["BookingError"] = "Thông tin đặt phòng không hợp lệ.";
                    return RedirectToAction("BookingPage", new { roomId });
                }

                if (checkInDate < DateTime.Today)
                {
                    TempData["BookingError"] = "Ngày check-in không thể là ngày trong quá khứ.";
                    return RedirectToAction("BookingPage", new { roomId });
                }

                if (checkOutDate <= checkInDate)
                {
                    TempData["BookingError"] = "Ngày check-out phải sau ngày check-in.";
                    return RedirectToAction("BookingPage", new { roomId });
                }

                var unavailableDates = new List<DateTime>();
                var start = checkInDate;
                var end = checkOutDate;

                for (var date = start; date <= end; date = date.AddDays(1))
                {
                    unavailableDates.Add(date);
                }

                // Existing code
                return View();
            }
            catch (Exception ex)
            {
                TempData["BookingError"] = "Có lỗi xảy ra khi xử lý thanh toán: " + ex.Message;
                return RedirectToAction("BookingPage", new { roomId });
            }
        }
    }
} 