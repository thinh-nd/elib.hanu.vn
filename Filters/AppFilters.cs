using QML.Web.UI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace QML.Web.UI.Filters
{
    public class LogAction : ActionFilterAttribute
    {
        EssentialController ec = new EssentialController();
        HanuELibraryEntities db = new HanuELibraryEntities();

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            RouteData routeData = filterContext.RouteData;
            ActionLog action = new ActionLog
            {
                user_id = ec.getUserId(),
                executed_user = ec.getUserName(),
                user_role = ec.getUserRole(ec.getUserId()),
                area_name = routeData.DataTokens["area"].ToString(),
                controller_name = routeData.Values["controller"].ToString(),
                action_name = routeData.Values["action"].ToString()
            };

            //if controller/action is ... then add action detail ... 
            //(description, object_ids, object_names)

            //apparently this method is useless since it can't get most of action detail

            db.ActionLogs.AddObject(action);
            db.SaveChanges();
        }

    }
}