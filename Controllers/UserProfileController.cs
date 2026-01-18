using BlogProject.Data;
using BlogProject.Entities;
using BlogProject.ViewModels;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity; 

namespace BlogProject.Controllers
{
	[Authorize]
	public class UserProfileController : Controller
	{
		Context db = new Context();

		public ActionResult Index()
		{
			if (Session["UserId"] == null) return RedirectToAction("Index", "Login");

			int userId = Convert.ToInt32(Session["UserId"]);
			var user = db.Users.Find(userId);

			bool hasPending = db.AuthorRequests.Any(x => x.UserId == userId && x.IsProcessed == false);
			ViewBag.HasPendingRequest = hasPending;

			var model = new UserProfileViewModel
			{
				User = user,
				MyComments = db.Comments
							   .Include(c => c.Post)
							   .Where(c => c.UserId == userId)
							   .OrderByDescending(c => c.Id)
							   .ToList(),

				LikedPosts = db.Likes
							   .Where(l => l.UserId == userId)
							   .Select(l => l.Post)
							   .ToList(),

				SavedPosts = db.SavedPosts
								.Where(s => s.UserId == userId)
								.Select(s => s.Post)
								.ToList()
			};

			return View(model);
		}

		public ActionResult DeleteComment(int id)
		{
			int userId = Convert.ToInt32(Session["UserId"]);
			var comment = db.Comments.FirstOrDefault(c => c.Id == id && c.UserId == userId);
			if (comment != null) { db.Comments.Remove(comment); db.SaveChanges(); }
			return RedirectToAction("Index");
		}

		public ActionResult RemoveLike(int postId)
		{
			int userId = Convert.ToInt32(Session["UserId"]);
			var like = db.Likes.FirstOrDefault(l => l.PostId == postId && l.UserId == userId);
			if (like != null) { db.Likes.Remove(like); db.SaveChanges(); }
			return RedirectToAction("Index");
		}

		public ActionResult RemoveSaved(int postId)
		{
			int userId = Convert.ToInt32(Session["UserId"]);
			var saved = db.SavedPosts.FirstOrDefault(s => s.PostId == postId && s.UserId == userId);
			if (saved != null) { db.SavedPosts.Remove(saved); db.SaveChanges(); }
			return RedirectToAction("Index");
		}

		[HttpPost]
		[Authorize]
		public ActionResult ApplyForAuthorship()
		{
			if (Session["UserId"] == null)
				return Json(new { success = false, message = "Oturum hatası." });

			int userId = (int)Session["UserId"];

			bool alreadyApplied = db.AuthorRequests.Any(x => x.UserId == userId && x.IsProcessed == false);
			if (alreadyApplied)
			{
				return Json(new { success = false, message = "Zaten bekleyen bir başvurunuz var. Lütfen sonuçlanmasını bekleyin." });
			}

			var user = db.Users.Include("Role").FirstOrDefault(u => u.Id == userId);

			if (user != null && (user.Role.Name == "Author" || user.Role.Name == "Admin"))
			{
				return Json(new { success = false, message = "Zaten yetkili bir kullanıcısınız." });
			}

			db.AuthorRequests.Add(new AuthorRequest
			{
				UserId = userId,
				CreatedAt = DateTime.Now, 
				IsProcessed = false,
				IsApproved = false
			});

			db.SaveChanges();

			return Json(new { success = true, message = "Yazarlık başvurunuz alındı. Yönetici onayından sonra paneliniz açılacaktır." });
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing) { db.Dispose(); }
			base.Dispose(disposing);
		}
	}
}