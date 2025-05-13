using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
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
            // Kiểm tra null hoặc rỗng
            if (string.IsNullOrWhiteSpace(CardNumber) || string.IsNullOrWhiteSpace(CVV))
            {
                return RedirectToAction("PaymentFailed");
            }

            // Kiểm tra nếu CardNumber hoặc CVV chứa ký tự không phải số (chữ cái, ký hiệu,...)
            if (!Regex.IsMatch(CardNumber, @"^\d+$") || !Regex.IsMatch(CVV, @"^\d+$"))
            {
                return RedirectToAction("PaymentFailed");
            }

            // Nếu hợp lệ
            return RedirectToAction("PaymentSuccess");
        }
    }
}