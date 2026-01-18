using BlogProject.Data;
using BlogProject.Entities;
using BlogProject.Helpers;
using BlogProject.ViewModels;
using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace BlogProject.Controllers
{
	[AllowAnonymous]
	public class LoginController : Controller
	{
		Context db = new Context();

		public ActionResult Index()
		{
			return View();
		}

		[HttpPost]
		public ActionResult Index(string username, string password)
		{
			var user = db.Users.Include("Role")
				.FirstOrDefault(u => u.Username == username);

			if (user != null)
			{
				if (user.LockoutEnd.HasValue)
				{
					if (user.LockoutEnd > DateTime.Now)
					{
						var remainingMinutes = (int)(user.LockoutEnd.Value - DateTime.Now).TotalMinutes;
						ViewBag.Error = "Çok fazla hatalı deneme yaptınız. Hesabınız güvenlik nedeniyle kilitlendi. Lütfen " + remainingMinutes + " dakika sonra tekrar deneyin.";
						return View();
					}
					else
					{
						user.LockoutEnd = null;
						user.FailedLoginAttempts = 0;
						db.SaveChanges();
					}
				}

				string hashedPassword = PasswordHasher.ComputeHash(password);

				if (user.PasswordHash == hashedPassword)
				{
					if (user.IsActive == false)
					{
						ViewBag.Error = "Hesabınız askıya alınmıştır. Yönetici ile iletişime geçin.";
						return View();
					}
					user.FailedLoginAttempts = 0;
					user.LockoutEnd = null;

					db.Sessions.Add(new Session
					{
						UserId = user.Id,
						CreatedAt = DateTime.UtcNow
					});

					db.SaveChanges();

					var authTicket = new FormsAuthenticationTicket(
							1,
							user.Username,
							DateTime.Now,
							DateTime.Now.AddMinutes(60),
							false,
							user.Role.Name
						);

					string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
					var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
					Response.Cookies.Add(authCookie);

					Session["UserId"] = user.Id;
					Session["Username"] = user.Username;
					Session["Role"] = user.Role.Name;

					if (user.Role.Name == "Admin") return RedirectToAction("Index", "Admin");
					if (user.Role.Name == "Author") return RedirectToAction("Index", "Author");
					return RedirectToAction("Index", "Default");
				}
				else
				{
					user.FailedLoginAttempts++;

					if (user.FailedLoginAttempts >= 3)
					{
						user.LockoutEnd = DateTime.Now.AddMinutes(15);
						db.SaveChanges();

						ViewBag.Error = "3 kez hatalı giriş yaptınız. Hesabınız 15 dakika süreyle kilitlendi.";
						return View();
					}
					else
					{
						int remainingRights = 3 - user.FailedLoginAttempts;
						db.SaveChanges();

						ViewBag.Error = "Şifre hatalı! Kalan hakkınız: " + remainingRights;
						return View();
					}
				}
			}
			ViewBag.Error = "Kullanıcı adı veya şifre hatalı!";
			return View();
		}

		[HttpGet]
		public ActionResult Register()
		{
			return View();
		}

		[HttpPost]
		public ActionResult Register(RegisterViewModel model)
		{
			var response = Request["g-recaptcha-response"];
			string secretKey = System.Configuration.ConfigurationManager.AppSettings["ReCaptchaSecret"];
			var client = new WebClient();

			var result = client.DownloadString("https://www.google.com/recaptcha/api/siteverify?secret=" + secretKey + "&response=" + response);

			if (!result.ToLower().Contains("\"success\": true"))
			{
				ViewBag.Error = "Lütfen robot olmadığınızı doğrulayın.";
				return View(model);
			}

			if (ModelState.IsValid)
			{
				var isExist = db.Users.Any(x => x.Username == model.Username || x.Email == model.Email);
				if (isExist)
				{
					ViewBag.Error = "Bu kullanıcı adı veya e-posta zaten kullanılıyor.";
					return View(model);
				}

				var userRole = db.Roles.FirstOrDefault(r => r.Name == "User");
				if (userRole == null)
				{
					ViewBag.Error = "Sistemde 'User' rolü tanımlı değil. Lütfen yöneticiye bildirin.";
					return View(model);
				}

				User newUser = new User();
				newUser.Username = model.Username;
				newUser.Email = model.Email;
				newUser.PasswordHash = PasswordHasher.ComputeHash(model.Password);
				newUser.IsActive = true;
				newUser.RoleId = userRole.Id;

				newUser.FailedLoginAttempts = 0;
				newUser.LockoutEnd = null;

				db.Users.Add(newUser);
				db.SaveChanges();

				TempData["Success"] = "Kayıt başarılı! Şimdi giriş yapabilirsiniz.";
				return RedirectToAction("Index");
			}

			return View(model);
		}

		public ActionResult LogOut()
		{
			if (Session["UserId"] != null)
			{
				int userId = (int)Session["UserId"];

				var activeSession = db.Sessions
					.Where(s => s.UserId == userId && s.EndedAt == null)
					.OrderByDescending(s => s.CreatedAt)
					.FirstOrDefault();

				if (activeSession != null)
				{
					activeSession.EndedAt = DateTime.UtcNow;
					db.SaveChanges();
				}
			}
			FormsAuthentication.SignOut();
			Session.Clear();
			Session.Abandon();
			return RedirectToAction("Index");
		}
		[HttpGet]
		public ActionResult ForgotPassword()
		{
			return View();
		}

		[HttpPost]
		public ActionResult ForgotPassword(string email)
		{
			var user = db.Users.FirstOrDefault(u => u.Email == email);
			if (user == null)
			{
				ViewBag.Error = "Bu e-posta adresiyle kayıtlı kullanıcı bulunamadı.";
				return View();
			}

			string token = Guid.NewGuid().ToString();
			user.ResetToken = token;
			user.ResetTokenExpires = DateTime.UtcNow.AddHours(1);
			db.SaveChanges();

			string resetLink = Url.Action("ResetPassword", "Login", new { token = token }, Request.Url.Scheme);

			try
			{
				BlogProject.Helpers.MailHelper.SendResetEmail(user.Email, resetLink);
				ViewBag.Success = "Sıfırlama linki e-posta adresinize gönderildi.";
			}
			catch (System.Exception)
			{
				ViewBag.Error = "Mail gönderilirken bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
			}

			return View();
		}

		[HttpGet]
		public ActionResult ResetPassword(string token)
		{
			var user = db.Users.FirstOrDefault(u => u.ResetToken == token && u.ResetTokenExpires > DateTime.UtcNow);

			if (user == null)
			{
				return RedirectToAction("Index");
			}

			ViewBag.Token = token;
			return View();
		}

		[HttpPost]
		public ActionResult ResetPassword(string token, string password, string rePassword)
		{
			if (password != rePassword)
			{
				ViewBag.Error = "Şifreler uyuşmuyor.";
				ViewBag.Token = token;
				return View();
			}

			var user = db.Users.FirstOrDefault(u => u.ResetToken == token && u.ResetTokenExpires > DateTime.UtcNow);

			if (user == null)
			{
				ViewBag.Error = "Linkin süresi dolmuş veya geçersiz.";
				return View();
			}

			user.PasswordHash = BlogProject.Helpers.PasswordHasher.ComputeHash(password);

			user.ResetToken = null;
			user.ResetTokenExpires = null;

			user.FailedLoginAttempts = 0;
			user.LockoutEnd = null;

			db.SaveChanges();

			TempData["Success"] = "Şifreniz başarıyla değiştirildi. Giriş yapabilirsiniz.";
			return RedirectToAction("Index");
		}
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				db.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}