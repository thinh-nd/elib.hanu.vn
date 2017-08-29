using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QML.Web.UI;
using QML.Library;
using QML.Library.Utilities;
using QML.Library.Helpers;
using QML.Library.Auth;
using QML.Library.Base.Controllers;
using QML.Library.Attributes;
using QML.Web.UI.Areas.DocumentAdmin.Helpers;
using QML.Web.UI.Areas.DocumentAdmin.Models;
using QML.Web.UI.Controllers;

namespace QML.Web.UI.Areas.DocumentAdmin.Controllers
{
    public class DocumentOrderController : SecuredController
    {
        private HanuELibraryEntities db = new HanuELibraryEntities();
        private EssentialController ec = new EssentialController();

        //
        // GET: /DocumentAdmin/DocumentOrder/
        [Menu(Title = "Xử lí yêu cầu", Path = "Yêu cầu tài liệu", Order = 3)]
        public ViewResult Index()
        {
            var documentorders = db.DocumentOrders.Include("auth_Users");
            return View(documentorders.ToList());
        }

        //
        // GET: /DocumentAdmin/DocumentOrder/Details/5

        public ViewResult Details(long id)
        {
            DocumentOrder documentorder = db.DocumentOrders.Single(d => d.OrderID == id);
            return View(documentorder);
        }

        //
        // GET: /DocumentAdmin/DocumentOrder/Delete/5
 
        public ActionResult Delete(long id)
        {
            DocumentOrder order = db.DocumentOrders.Single(d => d.OrderID == id);
            return View(order);
        }

        //
        // POST: /DocumentAdmin/DocumentOrder/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {            
            DocumentOrder order = db.DocumentOrders.Single(d => d.OrderID == id);
            db.DocumentOrders.DeleteObject(order);
            db.SaveChanges();

            //Logging
            System.Web.Routing.RouteData routeData = ControllerContext.RouteData;
            ActionLog action = new ActionLog
            {
                user_id = ec.getUserId(),
                executed_user = ec.getUserName(),
                user_role = ec.getUserRole(ec.getUserId()),

                area_name = routeData.DataTokens["area"].ToString(),
                controller_name = routeData.Values["controller"].ToString(),
                action_name = routeData.Values["action"].ToString(),
                executed_time = DateTime.Now,

                description = "xóa yêu cầu tài liệu",
                object_id = order.OrderID.ToString(),
                object_name = order.DocumentName,
                additional_info = order.OrderContent
            };

            db.ActionLogs.AddObject(action);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        //Update status of document order
        public ActionResult IsRead(int id)
        {
            DocumentOrder order = db.DocumentOrders.FirstOrDefault(p => p.OrderID == id);
            if (order.Status == false)
            {
                order.Status = true;
            }
            else
            {
                order.Status = false;
            }

            if (ModelState.IsValid)
            {
                db.SaveChanges();

                //Logging
                System.Web.Routing.RouteData routeData = ControllerContext.RouteData;

                string d;
                if (order.Status == true) d = "duyệt yêu cầu tài liệu";
                else d = "hủy duyệt yêu cầu tài liệu";

                ActionLog action = new ActionLog
                {
                    user_id = ec.getUserId(),
                    executed_user = ec.getUserName(),
                    user_role = ec.getUserRole(ec.getUserId()),

                    area_name = routeData.DataTokens["area"].ToString(),
                    controller_name = routeData.Values["controller"].ToString(),
                    action_name = routeData.Values["action"].ToString(),
                    executed_time = DateTime.Now,

                    description = d,
                    object_id = order.OrderID.ToString(),
                    object_name = order.OrderContent,
                    additional_info = null
                };

                db.ActionLogs.AddObject(action);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
    }
}