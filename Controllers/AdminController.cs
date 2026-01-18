using BlogProject.Data;
using BlogProject.Entities;
using BlogProject.ViewModels;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BlogProject.Controllers
{
	[Authorize(Roles = "Admin")]
	public class AdminController : Controller
	{
		Context db = new Context();

		public ActionResult Index()
		{
			DashboardViewModel model = new DashboardViewModel();

			model.TotalPosts = db.Posts.Count();
			model.TotalComments = db.Comments.Count();
			model.TotalUsers = db.Users.Count()-1;
			model.TotalLikes = db.Likes.Count();

			var categoryData = db.Posts
								 .GroupBy(p => p.Category.Title)
								 .Select(g => new
								 {
									 CategoryName = g.Key,
									 Count = g.Count()
								 })
								 .ToList();

			model.CategoryNames = categoryData.Select(x => x.CategoryName).ToList();
			model.CategoryPostCounts = categoryData.Select(x => x.Count).ToList();

			model.LastComments = db.Comments
								   .Include("User") 
								   .Include("Post")
								   .OrderByDescending(c => c.CreatedAt) 
								   .Take(5)
								   .Select(c => new CommentViewModel
								   {
									   Id = c.Id,
									   Username = c.User != null ? c.User.Username : "Silinmiş Üye",
									   Text = c.Text,
									   PostTitle = c.Post != null ? c.Post.Title : "Silinmiş Yazı",
									   CreatedAt = c.CreatedAt
								   })
								   .ToList();

			return View(model);
		}

		[HttpGet]
		public ActionResult PostList()
		{
			var posts = db.Posts
						  .Include(p => p.Category)
						  .OrderByDescending(p => p.Id)
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
		public ActionResult PostCreate(Post post, string tags)
		{
			if (ModelState.IsValid)
			{
				var user = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
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
			var post = db.Posts.Find(id);
			if (post == null) return HttpNotFound();

			ViewBag.Categories = new SelectList(db.Categories, "Id", "Title", post.CategoryId);
			return View(post);
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult PostEdit(Post model)
		{
			if (ModelState.IsValid)
			{
				var post = db.Posts.Find(model.Id);
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
			var post = db.Posts.Find(id);
			db.Posts.Remove(post);
			db.SaveChanges();
			return RedirectToAction("Index");
		}


		[HttpGet]
		public ActionResult CommentList()
		{
			var comments = db.Comments
							 .Include(c => c.User)
							 .Include(c => c.Post)
							 .Select(c => new CommentViewModel
							 {
								 Id = c.Id,
								 Username = c.User != null ? c.User.Username : "No User",
								 Text = c.Text,
								 PostId = c.PostId
							 })
							 .ToList();

			return View(comments);
		}
		public ActionResult CommentEdit(int id)
		{
			var comment = db.Comments.Find(id);
			if (comment == null) return HttpNotFound();
			return View(comment);
		}


		[HttpPost]
		public ActionResult CommentEdit(Comment comment)
		{
			if (ModelState.IsValid)
			{
				var dbComment = db.Comments.Find(comment.Id);
				if (dbComment != null)
				{
					dbComment.Text = comment.Text;
					db.SaveChanges();
				}
				return RedirectToAction("CommentList");
			}
			return View(comment);
		}

		public ActionResult CommentDelete(int id)
		{
			var comment = db.Comments.Find(id);
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
			var notifications = db.Notifications
				.Include(n => n.User)
				.OrderBy(n => n.IsRead)
				.ThenByDescending(n => n.CreatedAt)
				.ToList();

			return View(notifications);
		}

		public ActionResult ReadNotification(int id, bool goDetail = false)
		{
			var notif = db.Notifications.Find(id);
			if (notif != null)
			{
				notif.IsRead = true;
				db.SaveChanges();

				if (goDetail)
				{
					if (notif.Type == "NewPost" && notif.RelatedId.HasValue)
					{
						return RedirectToAction("Detail", "Post", new { id = notif.RelatedId });
					}
					else if (notif.Type == "NewComment" && notif.RelatedId.HasValue)
					{
						var comment = db.Comments.Find(notif.RelatedId);
						if (comment != null)
						{
							return RedirectToAction("Detail", "Post", new { id = comment.PostId });
						}
					}
				}
			}
			return RedirectToAction("NotificationList");
		}

		[HttpGet]
		public ActionResult UserList()
		{
			var users = db.Users
						  .Include(u => u.Role)
						  .Where(x => x.Role.Name != "Admin")
						  .ToList();

			return View(users);
		}

		public ActionResult ToggleAuthor(int id)
		{
			var user = db.Users.Include(u => u.Role).FirstOrDefault(u => u.Id == id);

			if (user != null)
			{
				if (user.Role.Name == "Author")
				{
					var userRole = db.Roles.FirstOrDefault(r => r.Name == "User" || r.Name == "Member");
					if (userRole != null) user.RoleId = userRole.Id;
				}
				else
				{
					var authorRole = db.Roles.FirstOrDefault(r => r.Name == "Author");
					if (authorRole != null) user.RoleId = authorRole.Id;
				}

				db.SaveChanges();
			}
			return RedirectToAction("UserList");
		}

		public ActionResult ToggleStatus(int id)
		{
			var user = db.Users.Find(id);
			if (user != null)
			{
				user.IsActive = !user.IsActive;
				db.SaveChanges();
			}
			return RedirectToAction("UserList");
		}

		[HttpGet]
		public ActionResult AboutUpdate()
		{
			var about = db.Abouts.FirstOrDefault();
			return View(about);
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult AboutUpdate(About p)
		{
			var about = db.Abouts.FirstOrDefault();
			if (about != null)
			{
				about.Title = p.Title;
				about.Content = p.Content;
				about.ImageUrl = p.ImageUrl;
				db.SaveChanges();
			}
			else
			{
				db.Abouts.Add(p);
				db.SaveChanges();
			}
			return RedirectToAction("AboutUpdate");
		}
		public ActionResult AuthorRequests()
		{
			var requests = db.AuthorRequests
							 .Include("User") 
							 .Where(x => x.IsProcessed == false)
							 .OrderByDescending(x => x.CreatedAt)
							 .ToList();

			return View(requests);
		}

		[HttpPost]
		public ActionResult ApproveAuthor(int id)
		{
			var req = db.AuthorRequests.Include("User").FirstOrDefault(x => x.Id == id);

			if (req != null)
			{
				req.IsProcessed = true;
				req.IsApproved = true;

				var authorRole = db.Roles.FirstOrDefault(x => x.Name == "Author");
				var user = db.Users.Find(req.UserId);

				if (authorRole != null && user != null)
				{
					user.RoleId = authorRole.Id; 
					db.SaveChanges();
					return Json(new { success = true, message = $"{user.Username} artık bir Yazar!" });
				}
			}
			return Json(new { success = false, message = "İşlem sırasında bir hata oluştu." });
		}

		[HttpPost]
		public ActionResult RejectAuthor(int id)
		{
			var req = db.AuthorRequests.Find(id);
			if (req != null)
			{
				req.IsProcessed = true;
				req.IsApproved = false;

				db.SaveChanges();
				return Json(new { success = true, message = "Başvuru reddedildi." });
			}
			return Json(new { success = false, message = "İstek bulunamadı." });
		}

		[HttpGet]
		public ActionResult GetCandidateDetails(int requestId)
		{
			var req = db.AuthorRequests.Include("User").FirstOrDefault(x => x.Id == requestId);
			if (req == null) return Json(new { success = false, message = "İstek bulunamadı" }, JsonRequestBehavior.AllowGet);

			var userId = req.UserId;

			var commentCount = db.Comments.Count(x => x.UserId == userId);
			var likeCount = db.Likes.Count(x => x.UserId == userId);

			var lastComments = db.Comments
								 .Where(x => x.UserId == userId)
								 .OrderByDescending(x => x.CreatedAt)
								 .Take(3)
								 .Select(x => new {
									 PostTitle = x.Post.Title,
									 Text = x.Text,
									 Date = x.CreatedAt.ToString()
								 })
								 .ToList();

			var data = new
			{
				success = true,
				username = req.User.Username,
				email = req.User.Email,
				commentCount = commentCount,
				likeCount = likeCount,
				lastComments = lastComments,
				requestId = req.Id 
			};

			return Json(data, JsonRequestBehavior.AllowGet);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing) { db.Dispose(); }
			base.Dispose(disposing);
		}
	}
}