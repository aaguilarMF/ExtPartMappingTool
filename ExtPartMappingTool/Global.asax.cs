using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Security.Principal;
using System.Security.Claims;
using System.Web.Http;

namespace ExtPartMappingTool
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        //protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        //{
        //    var authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
        //    if (authCookie != null)
        //    {
        //        FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
        //        if (authTicket != null && !authTicket.Expired)
        //        {
        //            var guat = authTicket.UserData;
        //            var roles = authTicket.UserData.Split(',');
        //            HttpContext.Current.User = new System.Security.Principal.GenericPrincipal(new FormsIdentity(authTicket), roles);
        //        }
        //    }
        //}
    }
}
