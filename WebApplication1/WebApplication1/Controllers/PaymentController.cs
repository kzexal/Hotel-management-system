using System;
using System.Linq;
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

        // [2] Xử lý thanh toán
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessPayment(FormCollection form)
        {
            try
            {
                // [A] Parse dữ liệu từ form
                if (!int.TryParse(form["roomId"], out int roomId) ||
                    !int.TryParse(form["totalPrice"], out int totalPrice) ||
                    !DateTime.TryParse(form["checkin"], out DateTime checkinDate) ||
                    !DateTime.TryParse(form["checkout"], out DateTime checkoutDate))
                {
                    TempData["BookingError"] = "Dữ liệu đầu vào không hợp lệ.";
                    return RedirectToAction("PaymentFail");
                }

                string paymentType = form["PaymentType"];
                if (string.IsNullOrEmpty(paymentType))
                {
                    TempData["BookingError"] = "Vui lòng chọn phương thức thanh toán.";
                    return RedirectToAction("PaymentFail");
                }

                // [B] Tách dữ liệu Guest ra biến riêng (để tránh lỗi LINQ)
                string email = form["GuestEmailAddress"];
                string contact = form["GuestContactNumber"];
                string firstName = form["GuestFirstName"];
                string lastName = form["GuestLastName"];
                string street = form["Street"];
                string city = form["City"];
                string zip = form["Zip"];

                // [C] Kiểm tra Guest đã tồn tại chưa
                var guest = db.Guests.FirstOrDefault(g =>
                    g.GuestEmailAddress == email &&
                    g.GuestContactNumber == contact &&
                    g.GuestFirstName == firstName &&
                    g.GuestLastName == lastName
                );

                // [D] Nếu chưa có thì thêm Guest mới
                if (guest == null)
                {
                    guest = new Guest
                    {
                        GuestFirstName = firstName,
                        GuestLastName = lastName,
                        GuestEmailAddress = email,
                        GuestContactNumber = contact,
                        Street = street,
                        City = city,
                        Zip = zip,
                        Status = "Reserved"
                    };
                    db.Guests.Add(guest);
                    db.SaveChanges();
                }

                // [E] Tạo Booking
                var booking = new Booking
                {
                    GuestId = guest.GuestId,
                    BookingDate = DateTime.Now,
                    CheckInDate = checkinDate,
                    CheckOutDate = checkoutDate,
                    BookingAmount = totalPrice,
                    Status = "Checkin"
                };
                db.Bookings.Add(booking);
                db.SaveChanges();

                // [F] Đánh dấu RoomBooked
                db.RoomBooked.Add(new RoomBooked
                {
                    BookingId = booking.BookingId,
                    RoomId = roomId
                });
                db.SaveChanges();

               
                // [H] Tạo Payment
                var payment = new Payment
                {
                    BookingId = booking.BookingId,
                    PaymentAmount = totalPrice,
                    PaymentStatus = "1", 
                    PaymentType = paymentType
                };
                db.Payments.Add(payment);
                db.SaveChanges();

                // [I] Thành công
                TempData["BookingSuccess"] = $"Đặt phòng và thanh toán thành công! Mã đặt: {booking.BookingId}";
                return RedirectToAction("PaymentSuccess");
            }
            catch (Exception ex)
            {
                // Ghi lỗi chi tiết
                string errorMessage = "Đã xảy ra lỗi khi xử lý thanh toán: " + ex.Message;
                if (ex.InnerException != null)
                    errorMessage += " | Inner: " + ex.InnerException.Message;
                if (ex.InnerException?.InnerException != null)
                    errorMessage += " | Inner.Inner: " + ex.InnerException.InnerException.Message;

                System.Diagnostics.Debug.WriteLine(errorMessage);

                TempData["BookingError"] = errorMessage;
                return RedirectToAction("PaymentFail");
            }
        }


        // [3] Trang thông báo lỗi
        public ActionResult PaymentFail()
        {
            ViewBag.Message = TempData["BookingError"];
            return View();
        }
        public ActionResult PaymentSuccess()
        {
            ViewBag.Message = TempData["BookingSuccess"];
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
