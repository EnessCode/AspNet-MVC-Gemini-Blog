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
		protected void Application_EndRequest()
		{
			if (Request.IsAuthenticated && Response.StatusCode == 302)
			{
				if (Response.RedirectLocation != null && Response.RedirectLocation.Contains("Login"))
				{
					Response.Clear();
					Response.StatusCode = 403;
					Response.Redirect("/Error/Page403");
					Response.End();
				}
			}
		}

		protected void Application_Error()
		{
			Exception ex = Server.GetLastError();

			if (ex != null)
			{
				string logPath = Server.MapPath("~/App_Data/ErrorLog.txt");
				string userName = User != null && User.Identity.IsAuthenticated ? User.Identity.Name : "Misafir";

				string content = $"-------------------\n" +
								 $"Tarih: {DateTime.Now}\n" +
								 $"Hata: {ex.Message}\n" +
								 $"Detay: {ex.StackTrace}\n" +
								 $"Kullanıcı: {userName}\n";

				System.IO.File.AppendAllText(logPath, content);
			}
		}
	}
}
