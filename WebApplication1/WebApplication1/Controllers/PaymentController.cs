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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ShowPaymentForm(int roomId, string checkin, string checkout, int totalPrice)
        {
            try
            {
                var room = db.Rooms.Find(roomId);
                if (room == null)
                {
                    TempData["BookingError"] = "Room not found.";
                    return RedirectToAction("PaymentFail");
                }

                // Store the original date strings in TempData
                TempData["BookingCheckIn"] = checkin;
                TempData["BookingCheckOut"] = checkout;
                TempData["BookingTotal"] = totalPrice;
                TempData["RoomId"] = roomId;

                return View("~/Views/Booking/Payment.cshtml", room);
            }
            catch (Exception ex)
            {
                TempData["BookingError"] = "An error occurred: " + ex.Message;
                return RedirectToAction("PaymentFail");
            }
        }

        // Handle payment processing
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessPayment(FormCollection form)
        {
            try
            {
                int roomId = 0;
                int totalPrice = 0;
                DateTime checkinDate = DateTime.MinValue;
                DateTime checkoutDate = DateTime.MinValue;

                if (!int.TryParse(form["roomId"], out roomId) ||
                    !int.TryParse(form["totalPrice"], out totalPrice) ||
                    !DateTime.TryParseExact(form["checkin"],
                        "dd/MM/yyyy",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out checkinDate) ||
                    !DateTime.TryParseExact(form["checkout"],
                        "dd/MM/yyyy",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out checkoutDate))
                {
                    TempData["BookingError"] = "Invalid input data.";
                    // Store form data in TempData
                    TempData["RoomId"] = roomId;
                    TempData["BookingCheckIn"] = form["checkin"];
                    TempData["BookingCheckOut"] = form["checkout"];
                    TempData["BookingTotal"] = totalPrice;
                    
                    // Store guest information
                    TempData["GuestFirstName"] = form["GuestFirstName"];
                    TempData["GuestLastName"] = form["GuestLastName"];
                    TempData["GuestEmailAddress"] = form["GuestEmailAddress"];
                    TempData["GuestContactNumber"] = form["GuestContactNumber"];
                    TempData["Street"] = form["Street"];
                    TempData["City"] = form["City"];
                    TempData["GuestId"] = form["GuestId"];
                    
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
                string guestId = form["GuestId"];
                
                if (string.IsNullOrEmpty(guestId))
                {
                    TempData["BookingError"] = "CCCD không được để trống.";
                    return RedirectToAction("PaymentFail");
                }

                var guest = db.Guests.FirstOrDefault(g => g.GuestId == guestId);

                if (guest == null)
                {
                    guest = new Guest
                    {
                        GuestId = guestId,
                        GuestFirstName = firstName,
                        GuestLastName = lastName,
                        GuestEmailAddress = email,
                        GuestContactNumber = contact,
                        Street = street,
                        City = city,
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
                            ServiceBookingDate = DateTime.Now
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
            
            // Get room information from TempData
            if (TempData["RoomId"] != null)
            {
                var roomId = Convert.ToInt32(TempData["RoomId"]);
                var room = db.Rooms.Find(roomId);
                return View(room);
            }
            
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
    