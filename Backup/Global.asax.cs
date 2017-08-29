using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using QML.Library.Base.Routing;
using QML.Library.Base.ViewEngines;
using QML.Library.Base;
using System.Web.Hosting;
using QML.Library.Auth;

namespace QML.Web.UI
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterViewEngine(ViewEngineCollection viewEngines)
        {
            // We do not need the default view engine
            viewEngines.Clear();

            var frontendViewEngine = new FrontendViewEngine
            {
                CurrentTheme = context => (new QML.Library.Base.ApplicationManager(context).GetCurrentApplication().CurrentTheme) ?? "Responsify"
            };

            viewEngines.Add(frontendViewEngine);
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.Add(new DomainBasedRoutingEngine()); // Parse area based on domain name
            routes.Add(new CustomRoutingEngine());      // Find in DB a custom url
            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            HostingEnvironment.RegisterVirtualPathProvider(new DbVirtualPathProvider());

            HtmlHelper.ClientValidationEnabled = true;
            HtmlHelper.UnobtrusiveJavaScriptEnabled = true;

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            ControllerBuilder.Current.DefaultNamespaces.Add("QML.Website.Controllers");

            //RegisterViewEngine(ViewEngines.Engines);
        }
    }
}