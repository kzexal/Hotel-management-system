using System;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class PaymentController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

     
        // [1] Hiển thị form thanh toán
        public ActionResult ShowPaymentForm(int roomId, DateTime checkin, DateTime checkout, int totalPrice)
        {
            var room = db.Rooms.Find(roomId);
            if (room == null)
                return RedirectToAction("PaymentFail");

            ViewBag.CheckIn = checkin;
            ViewBag.CheckOut = checkout;
            ViewBag.TotalPrice = totalPrice;

            return View("~/Views/Booking/Payment.cshtml", room); // View dùng @model Room
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessPayment(FormCollection form)
        {
            try
            {
                if (!int.TryParse(form["roomId"], out int roomId) ||
                    !int.TryParse(form["totalPrice"], out int totalPrice) ||
                    !DateTime.TryParse(form["checkin"], out DateTime checkinDate) ||
                    !DateTime.TryParse(form["checkout"], out DateTime checkoutDate))
                {
                    TempData["BookingError"] = "Invalid input data.";
                    return RedirectToAction("PaymentFail");
                }

                string paymentType = form["PaymentType"];
                if (string.IsNullOrEmpty(paymentType))
                {
                    TempData["BookingError"] = "Please select a payment method.";
                    return RedirectToAction("PaymentFail");
                }

                // [1] Lưu thông tin Guest
                var guest = new Guest
                {
                    GuestFirstName = form["GuestFirstName"],
                    GuestLastName = form["GuestLastName"],
                    GuestEmailAddress = form["GuestEmailAddress"],
                    GuestContactNumber = form["GuestContactNumber"],
                    Street = form["Street"],
                    City = form["City"],
                    Zip = form["Zip"],
                    Status = "Active"
                };
                db.Guests.Add(guest);
                db.SaveChanges();

                // [2] Tạo Booking
                var booking = new Booking
                {
                    GuestId = guest.GuestId,
                    BookingDate = DateTime.Now,
                    CheckInDate = checkinDate,
                    CheckOutDate = checkoutDate,
                    BookingAmount = totalPrice,
                    Status = "Confirmed" // ✅ Đặt luôn là Confirmed
                };
                db.Bookings.Add(booking);
                db.SaveChanges();

                // [3] Đánh dấu Room đã được đặt
                db.RoomBooked.Add(new RoomBooked
                {
                    BookingId = booking.BookingId,
                    RoomId = roomId
                });

                var room = db.Rooms.Find(roomId);
                if (room != null)
                {
                    room.Available = "No";
                }
                db.SaveChanges();

                // [4] Tạo bản ghi Payment (thanh toán giả lập thành công)
                var payment = new Payment
                {
                    BookingId = booking.BookingId,
                    PaymentAmount = totalPrice,
                    PaymentStatus = "Success",
                    PaymentType = paymentType
                };
                db.Payments.Add(payment);
                db.SaveChanges();

                TempData["BookingSuccess"] = $"Đặt phòng và thanh toán thành công! Mã đặt: {booking.BookingId}";
                return RedirectToAction("BookingSuccess", "Booking");
            }
            catch (Exception)
            {
                TempData["BookingError"] = "Đã xảy ra lỗi khi xử lý thanh toán.";
                return RedirectToAction("PaymentFail");
            }
        }

        public ActionResult PaymentFail()
        {
            ViewBag.Message = TempData["BookingError"];
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
