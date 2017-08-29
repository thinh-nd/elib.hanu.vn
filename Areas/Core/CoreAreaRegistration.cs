using System.Web.Mvc;

namespace QML.Web.UI.Areas.Core
{
    public class CoreAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Core";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "HomeAdmin",
                "Core/Home/Settings",
                new { controller = "HomeAdmin", action = "Settings"}
            );
        }
    }
}
