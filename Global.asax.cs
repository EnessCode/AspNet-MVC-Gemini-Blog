using System;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace BlogProject
{
	public class MvcApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);
		}

		protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
		{
			var authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
			if (authCookie != null)
			{
				FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
				if (authTicket != null && !authTicket.Expired)
				{
					var roles = authTicket.UserData.Split(',');
					HttpContext.Current.User = new GenericPrincipal(new FormsIdentity(authTicket), roles);
				}
			}
		}

		protected void Application_Error()
		{
			Exception ex = Server.GetLastError();

			string logPath = Server.MapPath("~/App_Data/ErrorLog.txt");
			string content = $"-------------------\n" +
							 $"Tarih: {DateTime.Now}\n" +
							 $"Hata: {ex.Message}\n" +
							 $"Detay: {ex.StackTrace}\n" +
							 $"Kullanıcı: {User.Identity.Name}\n";

			System.IO.File.AppendAllText(logPath, content);
		}
	}
}
