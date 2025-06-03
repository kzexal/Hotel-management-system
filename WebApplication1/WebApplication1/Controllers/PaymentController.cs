using System;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class PaymentController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // Hiển thị form thanh toán
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

        // Xử lý thanh toán
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
                    TempData["BookingError"] = "Dữ liệu đầu vào không hợp lệ.";
                    return RedirectToAction("PaymentFail");
                }

                string paymentType = form["PaymentType"];
                if (string.IsNullOrEmpty(paymentType))
                {
                    TempData["BookingError"] = "Vui lòng chọn phương thức thanh toán.";
                    return RedirectToAction("PaymentFail");
                }

                string email = form["GuestEmailAddress"];
                string contact = form["GuestContactNumber"];
                string firstName = form["GuestFirstName"];
                string lastName = form["GuestLastName"];
                string street = form["Street"];
                string city = form["City"];
                string zip = form["Zip"];

                var guest = db.Guests.FirstOrDefault(g =>
                    g.GuestEmailAddress == email &&
                    g.GuestContactNumber == contact &&
                    g.GuestFirstName == firstName &&
                    g.GuestLastName == lastName
                );

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
                        Status = "Reserved",
                        UserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : (int?)null
                    };
                    db.Guests.Add(guest);
                    db.SaveChanges();
                }

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

                db.RoomBooked.Add(new RoomBooked
                {
                    BookingId = booking.BookingId,
                    RoomId = roomId
                });
                db.SaveChanges();

                db.Payments.Add(new Payment
                {
                    BookingId = booking.BookingId,
                    PaymentAmount = totalPrice,
                    PaymentStatus = "1",
                    PaymentType = paymentType
                });
                db.SaveChanges();

                TempData["BookingSuccess"] = $"Đặt phòng và thanh toán thành công! Mã đặt: {booking.BookingId}";
                return RedirectToAction("PaymentSuccess");
            }
            catch (Exception ex)
            {
                string errorMessage = GetFullErrorMessage(ex);
                System.Diagnostics.Debug.WriteLine("Lỗi khi thanh toán: " + errorMessage);
                TempData["BookingError"] = "Đã xảy ra lỗi khi xử lý thanh toán: " + errorMessage;
                return RedirectToAction("PaymentFail");
            }
        }

        // Xử lý thanh toán dịch vụ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessServicePayment(int bookingId, string PaymentType, int[] selectedServiceIds)
        {
            try
            {
                var booking = db.Bookings.FirstOrDefault(b => b.BookingId == bookingId);
                if (booking == null || selectedServiceIds == null || selectedServiceIds.Length == 0)
                {
                    TempData["ServiceError"] = "Không tìm thấy booking hoặc chưa chọn dịch vụ.";
                    return RedirectToAction("UserDashboard", "User");
                }

                int totalCost = 0;
                foreach (var serviceId in selectedServiceIds)
                {
                    var service = db.Services.Find(serviceId);
                    if (service != null)
                    {
                        totalCost += service.ServiceCost;

                        db.ServicesUsed.Add(new ServicesUsed
                        {
                            BookingId = bookingId,
                            ServiceId = serviceId
                        });
                        db.SaveChanges();
                    }
                }

                booking.BookingAmount += totalCost;
                db.SaveChanges();

                db.Payments.Add(new Payment
                {
                    BookingId = booking.BookingId,
                    PaymentAmount = totalCost,
                    PaymentStatus = "1",
                    PaymentType = PaymentType
                });
                db.SaveChanges();

                TempData["ServiceSuccess"] = "Dịch vụ đã được thanh toán thành công.";
            }
            catch (Exception ex)
            {
                string errorMessage = GetFullErrorMessage(ex);
                System.Diagnostics.Debug.WriteLine("Lỗi dịch vụ: " + errorMessage);
                TempData["ServiceError"] = "Lỗi khi xử lý thanh toán dịch vụ: " + errorMessage;
            }

            return RedirectToAction("UserDashboard", "User");
        }

        // Trang lỗi
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

        // Hàm xử lý lỗi chi tiết
        private string GetFullErrorMessage(Exception ex)
        {
            string message = ex.Message;
            Exception inner = ex.InnerException;
            while (inner != null)
            {
                message += " | Inner: " + inner.Message;
                inner = inner.InnerException;
            }
            return message;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
