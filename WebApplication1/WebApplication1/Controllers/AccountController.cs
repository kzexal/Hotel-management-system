    using System.Collections.Generic;
    using System.Web.Mvc;
    using WebApplication1.Models;

    namespace WebApplication1.Controllers
    {
        public class AccountController : Controller
        {
            // Fake user data for demo
            private readonly Dictionary<string, string> users = new Dictionary<string, string>
            {
                { "admin", "123456" },
                { "user", "password" }
            };

            [HttpGet]
            public ActionResult Login()
            {
                return View();
            }

            [HttpPost]
            public JsonResult Login(LoginViewModel model)
            {
                if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
                {
                    return Json(new { success = false, message = "Please enter username and password" });
                }

                if (users.ContainsKey(model.Username) && users[model.Username] == model.Password)
                {
                    Session["Username"] = model.Username;
                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "Invalid username or password" });
            }

            public ActionResult Logout()
            {
                Session.Clear();
                return RedirectToAction("Login", "Account");
            }
        }
    }
