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

            // Lấy danh sách khoảng ngày đã được đặt (CheckIn -> CheckOut)
            var bookings = db.RoomBooked
                .Where(rb => rb.RoomId == roomId)
                .Join(db.Bookings, rb => rb.BookingId, b => b.BookingId, (rb, b) => new { b.CheckInDate, b.CheckOutDate })
                .ToList(); // Chạy EF tại đây → sau đó xử lý bằng LINQ to Objects

            // Duyệt từng booking, cộng thêm 1 ngày trước và 1 ngày sau (ngày cách ly)
            var unavailableDates = new HashSet<string>();
            foreach (var booking in bookings)
            {
                var start = booking.CheckInDate.AddDays(-1);
                var end = booking.CheckOutDate; // không +1 vì CheckOut không còn ở

                for (var date = start; date <= end; date = date.AddDays(1))
                {
                    unavailableDates.Add(date.ToString("yyyy-MM-dd"));
                }
            }

            ViewBag.UnavailableDates = unavailableDates.ToList();
            return View(room); // View: BookingPage.cshtml
        }

        // [2] Nhận dữ liệu từ form và chuyển sang trang thanh toán
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
