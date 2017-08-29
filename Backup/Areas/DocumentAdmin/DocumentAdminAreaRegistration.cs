using System.Web.Mvc;

namespace QML.Web.UI.Areas.DocumentAdmin
{
    public class DocumentAdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "DocumentAdmin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "DocumentAdmin_default",
                "DocumentAdmin/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
