using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // === LOGIN ===
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Please enter username and password" });
            }

            var user = db.Logins.FirstOrDefault(u => u.Username == model.Username && u.Password == model.Password);

            if (user != null)
            {
                Session["Username"] = user.Username;
                Session["TypeAccount"] = user.TypeAccount;
                Session["UserId"] = user.LoginId;
                @Session["Role"] = user.TypeAccount;
                return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
            }

            return Json(new { success = false, message = "Invalid username or password" });
        }

        // === LOGOUT ===
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        // === REGISTER ===
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Please fill in all required fields." });
            }

            var exists = db.Logins.Any(u => u.Username == model.Username);
            if (exists)
            {
                return Json(new { success = false, message = "Username already exists." });
            }

            var newUser = new Login
            {
                Username = model.Username,
                Password = model.Password,
                TypeAccount = model.TypeAccount,
                NewUser = "Yes" 
            };

            db.Logins.Add(newUser);
            db.SaveChanges();

            return Json(new { success = true, redirectUrl = Url.Action("Login", "Account") });
        }
    }
}
