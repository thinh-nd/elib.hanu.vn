using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QML.Web.UI.Areas.Core.Controllers
{
    public class RoutingController : Controller
    {
        //
        // GET: /Core/Routing/

        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult Route(String controller, String action, String id)
        {
            if (controller == "FirstRun") return RedirectToAction("Index", "HomeAdmin");
            else return View();

            if (controller == "HomeAdmin" && action == "Index") return RedirectToAction("Document", "DocumentAdmin");
        }

        
    }
}
