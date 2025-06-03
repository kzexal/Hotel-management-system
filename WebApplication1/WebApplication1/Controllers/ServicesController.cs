using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class ServiceController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        [HttpGet]
        public JsonResult GetServices()
        {
            var services = db.Services.Select(s => new
            {
                s.ServiceId,
                s.ServiceName,
                s.ServiceCost
            }).ToList();

            return Json(services, JsonRequestBehavior.AllowGet);
        }

      
    }
}
