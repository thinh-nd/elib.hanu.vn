using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QML.Web.UI.Helpers
{
	public class MenuHelper
	{
        public static string IsCurrent(string Action, ViewContext helper)
        {            
            string controller = helper.RouteData.Values["controller"].ToString();
            string action = helper.RouteData.Values["action"].ToString();            

            if (action.Equals(Action)) return @"id=active";
            else return null;
        }
        
	}
}