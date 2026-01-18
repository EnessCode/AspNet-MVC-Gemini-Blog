using BlogProject.Data;
using BlogProject.Entities;
using BlogProject.ViewModels;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace BlogProject.Controllers
{
	[Authorize(Roles = "Author")]
	public class AuthorController : Controller
	{
		Context db = new Context();

		public ActionResult Index()
		{
			string username = User.Identity.Name;
			var user = db.Users.FirstOrDefault(u => u.Username == username);
			int userId = user.Id;

			var posts = db.Posts
						  .Where(p => p.UserId == userId)
						  .Include(p => p.Category)
						  .ToList();
			return View(posts);
		}

		[HttpGet]
		public ActionResult PostCreate()
		{
			ViewBag.Categories = new SelectList(db.Categories, "Id", "Title");
			return View();
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		[ValidateInput(false)]
		public ActionResult PostCreate(Post post, string tags)
		{
			if (ModelState.IsValid)
			{
				string username = User.Identity.Name;
				var user = db.Users.FirstOrDefault(u => u.Username == username);

				post.UserId = user.Id;
				post.CreatedAt = DateTime.UtcNow;

				db.Posts.Add(post);
				db.SaveChanges();

				if (!string.IsNullOrEmpty(tags))
				{
					string[] tagArray = tags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

					foreach (var tagName in tagArray)
					{
						var cleanTagName = tagName.Trim();

						var existingTag = db.Tags.FirstOrDefault(t => t.Name == cleanTagName);

						if (existingTag == null)
						{
							existingTag = new Tag { Name = cleanTagName };
							db.Tags.Add(existingTag);
							db.SaveChanges();
						}

						var postTag = new PostTag
						{
							PostId = post.Id,
							TagId = existingTag.Id
						};
						db.PostTags.Add(postTag);
					}
					db.SaveChanges();
				}
				return RedirectToAction("Index");
			}

			ViewBag.Categories = new SelectList(db.Categories, "Id", "Title", post.CategoryId);
			return View(post);
		}

		[HttpGet]
		public ActionResult PostEdit(int id)
		{
			string username = User.Identity.Name;

			var post = db.Posts.FirstOrDefault(p => p.Id == id && p.User.Username == username);

			if (post == null) return HttpNotFound();

			ViewBag.Categories = new SelectList(db.Categories, "Id", "Title", post.CategoryId);
			return View(post);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult PostEdit(Post model)
		{
			if (ModelState.IsValid)
			{
				string username = User.Identity.Name;
				var post = db.Posts.FirstOrDefault(p => p.Id == model.Id && p.User.Username == username);

				if (post == null) return HttpNotFound();

				post.Title = model.Title;
				post.Content = model.Content;
				post.ImageUrl = model.ImageUrl;
				post.CategoryId = model.CategoryId;

				db.SaveChanges();
				return RedirectToAction("Index");
			}
			ViewBag.Categories = new SelectList(db.Categories, "Id", "Title", model.CategoryId);
			return View(model);
		}


		[HttpGet]
		public ActionResult PostDelete(int id)
		{
			string username = User.Identity.Name;

			var post = db.Posts.FirstOrDefault(p => p.Id == id && p.User.Username == username);

			if (post != null)
			{
				db.Posts.Remove(post);
				db.SaveChanges();
			}
			return RedirectToAction("Index");
		}

		[HttpGet]
		public ActionResult CommentList()
		{
			string username = User.Identity.Name;
			var user = db.Users.FirstOrDefault(u => u.Username == username);
			int userId = user.Id;
			var comments = db.Comments
							 .Include(c => c.User)
							 .Include(c => c.Post) 
							 .Where(c => c.Post.UserId == userId)
							 .OrderByDescending(c => c.Id)
							 .Select(c => new CommentViewModel
							 {
								 Id = c.Id,
								 Username = c.User != null ? c.User.Username : "Misafir",
								 Text = c.Text,
								 PostId = c.PostId,

							 })
							 .ToList();

			return View(comments);
		}

		public ActionResult CommentDelete(int id)
		{
			string username = User.Identity.Name;
			var user = db.Users.FirstOrDefault(u => u.Username == username);
			int userId = user.Id; 
			var comment = db.Comments
							.Include(c => c.Post)
							.FirstOrDefault(c => c.Id == id && c.Post.UserId == userId);

			if (comment != null)
			{
				db.Comments.Remove(comment);
				db.SaveChanges();
			}
			return RedirectToAction("CommentList");
		}

		[HttpGet]
		public ActionResult NotificationList()
		{
			string username = User.Identity.Name;
			var user = db.Users.FirstOrDefault(u => u.Username == username);
			int userId = user.Id;
			var notifications = db.Notifications
				.Include(n => n.User)
				.Where(n => n.UserId == userId && n.Type == "NewComment")
				.OrderBy(n => n.IsRead)
				.ThenByDescending(n => n.CreatedAt)
				.ToList();

			return View(notifications);
		}

		public ActionResult ReadNotification(int id, bool goDetail = false)
		{
			string username = User.Identity.Name;
			var user = db.Users.FirstOrDefault(u => u.Username == username);
			int userId = user.Id;

			var notif = db.Notifications.FirstOrDefault(n => n.Id == id && n.UserId == userId);

			if (notif != null)
			{
				notif.IsRead = true;
				db.SaveChanges();

				if (goDetail)
				{
					if (notif.Type == "NewComment" && notif.RelatedId.HasValue)
					{
						var comment = db.Comments.Find(notif.RelatedId);

						if (comment != null)
						{
							return RedirectToAction("Detail", "Post", new { id = comment.PostId });
						}
						else
						{
							TempData["Error"] = "Bu yorum silinmiş olabilir.";
						}
					}

				}
			}
			return RedirectToAction("NotificationList");
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
