using System;
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

            return View(room); // View nhận @model Room
        }

        // [2] Lưu dữ liệu booking tạm thời rồi chuyển đến trang thanh toán
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

                // Lưu tạm thông tin để sử dụng ở bước thanh toán
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
