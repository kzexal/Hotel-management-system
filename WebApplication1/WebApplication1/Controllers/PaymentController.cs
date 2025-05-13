using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class PaymentController : Controller
    {
        // GET: Payment
        public ActionResult PaymentFailed()
        {
            return View();
        }
        public ActionResult PaymentSuccess()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ProcessPayment(string CardNumber, string Bank, string ExpDate, string CVV)
        {
            
            if (CardNumber == "1234123412341234" && CVV == "123")
            {
                return RedirectToAction("PaymentSuccess");
            }
            else
            {
                return RedirectToAction("PaymentFailed");
            }
        }

    }
}