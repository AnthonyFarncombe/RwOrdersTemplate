using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using RwOrders.Models;
using System.Data.Entity;
using System.Text;

namespace RwOrders.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index() => View();
    }
}