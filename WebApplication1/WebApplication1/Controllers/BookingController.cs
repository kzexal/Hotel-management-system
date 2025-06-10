using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class BookingController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // [1] Hiển thị trang đặt phòng
        public ActionResult BookingPage(int roomId)
        {  
            var room = db.Rooms.Find(roomId);
            if (room == null)
                return HttpNotFound();
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Request.RawUrl });
            }

            // Lấy tất cả các booking hiện tại và trong tương lai của phòng này
            var bookings = db.RoomBooked
                .Where(rb => rb.RoomId == roomId && rb.Booking.Status != "Cancelled")
                .Select(rb => new { rb.Booking.CheckInDate, rb.Booking.CheckOutDate })
                .ToList();

            var disabledDates = new HashSet<string>();
            foreach (var booking in bookings)
            {
                var start = booking.CheckInDate;
                var end = booking.CheckOutDate;

                if (start <= end && start != DateTime.MinValue && end != DateTime.MinValue)
                {
                    // Thêm tất cả các ngày từ check-in đến check-out vào danh sách
                    for (var date = start; date <= end; date = date.AddDays(1))
                    {
                        disabledDates.Add(date.ToString("dd/MM/yyyy"));
                    }
                }
            }

            ViewBag.DisabledDates = disabledDates.ToList();
            return View(room);
        }

        // [2] Kiểm tra tính khả dụng của phòng
        [HttpPost]
        public JsonResult CheckRoomAvailability(int roomId, string checkinDate, string checkoutDate)
        {
            try
            {
                if (string.IsNullOrEmpty(checkinDate) || string.IsNullOrEmpty(checkoutDate))
                {
                    return Json(new { success = false, message = "Dates cannot be empty" });
                }

                var dateFormat = "dd/MM/yyyy";
                var culture = System.Globalization.CultureInfo.InvariantCulture;
                
                if (!DateTime.TryParseExact(checkinDate, dateFormat, 
                    culture,
                    System.Globalization.DateTimeStyles.None, 
                    out DateTime parsedCheckin) ||
                    !DateTime.TryParseExact(checkoutDate, dateFormat,
                    culture,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime parsedCheckout))
                {
                    return Json(new { success = false, message = "Invalid date format. Use dd/MM/yyyy" });
                }

                // Kiểm tra xem phòng có được đặt trong khoảng thời gian này không
                var isBooked = db.RoomBooked
                    .Any(rb => rb.RoomId == roomId &&
                         rb.Booking.Status != "Cancelled" &&
                         ((rb.Booking.CheckInDate <= parsedCheckin && rb.Booking.CheckOutDate > parsedCheckin) ||
                          (rb.Booking.CheckInDate < parsedCheckout && rb.Booking.CheckOutDate >= parsedCheckout) ||
                          (rb.Booking.CheckInDate >= parsedCheckin && rb.Booking.CheckOutDate <= parsedCheckout)));

                return Json(new { success = true, isAvailable = !isBooked });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // [3] Nhận dữ liệu từ form và chuyển sang trang thanh toán
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Payment(FormCollection form)
        {
            try
            {
                if (!int.TryParse(form["RoomId"], out int roomId) ||
                    !int.TryParse(form["BookingAmount"], out int totalPrice) ||
                    !DateTime.TryParse(form["CheckInDate"], out DateTime checkinDate) ||
                    !DateTime.TryParse(form["CheckOutDate"], out DateTime checkoutDate))
                {
                    TempData["BookingError"] = "Thông tin đặt phòng không hợp lệ.";
                    return RedirectToAction("BookingPage", new { roomId });
                }

                // Lưu thông tin tạm thời qua TempData
                TempData["RoomId"] = roomId;
                TempData["BookingCheckIn"] = checkinDate;
                TempData["BookingCheckOut"] = checkoutDate;
                TempData["BookingTotal"] = totalPrice;

                return RedirectToAction("ShowPaymentForm", "Payment", new
                {
                    roomId = roomId,
                    checkin = checkinDate,
                    checkout = checkoutDate,
                    totalPrice = totalPrice
                });
            }
            catch (Exception)
            {
                TempData["BookingError"] = "Đã xảy ra lỗi khi xử lý đặt phòng.";
                return RedirectToAction("BookingPage", new { roomId = form["RoomId"] });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
