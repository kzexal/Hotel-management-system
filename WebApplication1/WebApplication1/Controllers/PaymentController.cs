using System;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class PaymentController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // Display the payment form
        public ActionResult ShowPaymentForm(int roomId, DateTime checkin, DateTime checkout, int totalPrice)
        {
            var room = db.Rooms.Find(roomId);
            if (room == null)
                return RedirectToAction("PaymentFail");

            ViewBag.CheckIn = checkin;
            ViewBag.CheckOut = checkout;
            ViewBag.TotalPrice = totalPrice;

            return View("~/Views/Booking/Payment.cshtml", room); // View uses @model Room
        }

        // Handle payment processing
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

                TempData["BookingSuccess"] = $"Booking and payment successful! Booking ID: {booking.BookingId}";
                return RedirectToAction("PaymentSuccess");
            }
            catch (Exception ex)
            {
                string errorMessage = GetFullErrorMessage(ex);
                System.Diagnostics.Debug.WriteLine("Error during payment: " + errorMessage);
                TempData["BookingError"] = "An error occurred while processing the payment: " + errorMessage;
                return RedirectToAction("PaymentFail");
            }
        }

        // Handle service payment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessServicePayment(int bookingId, string PaymentType, int[] selectedServiceIds)
        {
            try
            {
                var booking = db.Bookings.FirstOrDefault(b => b.BookingId == bookingId);
                if (booking == null || selectedServiceIds == null || selectedServiceIds.Length == 0)
                {
                    TempData["ServiceError"] = "Booking not found or no services selected.";
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
                            ServiceId = serviceId,
                            ServiceBookingDate = DateTime.Now,
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

                TempData["ServiceSuccess"] = "Service payment was successful.";
            }
            catch (Exception ex)
            {
                string errorMessage = GetFullErrorMessage(ex);
                System.Diagnostics.Debug.WriteLine("Service payment error: " + errorMessage);
                TempData["ServiceError"] = "Error while processing service payment: " + errorMessage;
            }

            return RedirectToAction("UserDashboard", "User");
        }

        // Error page
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

        // Detailed error message handler
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
    