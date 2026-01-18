using BlogProject.Data;
using BlogProject.Entities;
using BlogProject.Helpers;
using BlogProject.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BlogProject.Controllers
{
	public class PostController : Controller
	{
		Context db = new Context();

		public ActionResult Index(int? id, string search, int page = 1)
		{
			var vm = new PostDetailViewModel();
			int pageSize = 5;

			var query = db.Posts.Include(p => p.PostTags.Select(pt => pt.Tag)).AsQueryable();

			if (id != null)
			{
				query = query.Where(x => x.CategoryId == id);
			}

			if (!string.IsNullOrEmpty(search))
			{
				query = query.Where(x =>
					x.PostTags.Any(pt => pt.Tag.Name.Contains(search)) ||
					x.Title.Contains(search)
				);
			}

			int totalPosts = query.Count();
			int totalPages = (int)Math.Ceiling((double)totalPosts / pageSize);

			if (page < 1) page = 1;
			if (page > totalPages && totalPages > 0) page = totalPages;

			vm.Posts = query
				.OrderByDescending(x => x.CreatedAt)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToList();

			vm.RecentPosts = db.Posts
				.OrderByDescending(x => x.CreatedAt)
				.Take(3)
				.ToList();

			ViewBag.SearchTerm = search;
			ViewBag.CategoryId = id; 
			ViewBag.CurrentPage = page;
			ViewBag.TotalPages = totalPages;

			return View(vm);
		}

		public ActionResult Detail(int id)
		{
			var vm = new PostDetailViewModel();

			vm.Posts = db.Posts
				.Where(p => p.Id == id)
				.ToList();

			vm.Comments = db.Comments
				.Where(c => c.PostId == id)
				.OrderByDescending(c => c.CreatedAt)
				.ToList();

			vm.RecentPosts = db.Posts
				.OrderByDescending(p => p.CreatedAt)
				.Take(3)
				.ToList();

			return View(vm);
		}

		[HttpGet]
		public PartialViewResult AddComment(int postId)
		{
			ViewBag.PostId = postId;
			return PartialView();
		}

		[Authorize]
		[HttpPost]
		public async Task<ActionResult> AddComment(int PostId, string Text)
		{
			if (Session["UserId"] == null)
			{
				return Json(new { success = false, message = "Oturum süreniz dolmuş, lütfen tekrar giriş yapın." });
			}

			if (string.IsNullOrWhiteSpace(Text))
			{
				return Json(new { success = false, message = "Yorum alanı boş bırakılamaz." });
			}

			string aiResult = await GeminiModerator.CheckContent(Text);

			if (aiResult == "RET")
			{
				return Json(new { success = false, message = "Yorumunuz topluluk kurallarına aykırı (Hakaret/Argo) bulunduğu için engellendi." });
			}

			if (aiResult.StartsWith("HATA"))
			{
				return Json(new { success = false, message = "Yorum denetlenirken bir sistem hatası oluştu. Lütfen daha sonra tekrar deneyin." });
			}

			int userId = (int)Session["UserId"];

			db.Comments.Add(new Comment
			{
				PostId = PostId,
				UserId = userId,
				Text = Text,
				CreatedAt = DateTime.Now
			});

			db.SaveChanges();

			return Json(new { success = true, message = "Yorumunuz başarıyla paylaşıldı." });
		}


		[Authorize]
		[HttpPost]
		public ActionResult LikePost(int postId)
		{
			int userId = (int)Session["UserId"];

			bool alreadyLiked = db.Likes.Any(l => l.PostId == postId && l.UserId == userId);
			if (alreadyLiked)
			{
				TempData["Error"] = "Bu postu zaten beğendiniz.";
				return RedirectToAction("Detail", new { id = postId });
			}

			db.Likes.Add(new Like
			{
				PostId = postId,
				UserId = userId,
				CreatedAt = DateTime.Now
			});

			db.SaveChanges();
			return RedirectToAction("Detail", new { id = postId });
		}

		[HttpPost]
		[Authorize]
		public ActionResult SavePost(int postId)
		{
			if (Session["UserId"] == null)
			{
				return RedirectToAction("Login", "Login");
			}

			int userId = (int)Session["UserId"];

			bool alreadySaved = db.SavedPosts
				.Any(s => s.PostId == postId && s.UserId == userId);

			if (alreadySaved)
			{
				TempData["SaveError"] = "Bu postu zaten kaydettiniz.";
				return RedirectToAction("Detail", new { id = postId });
			}

			var savedPost = new SavedPost
			{
				PostId = postId,
				UserId = userId,
				CreatedAt = DateTime.Now
			};

			db.SavedPosts.Add(savedPost);
			db.SaveChanges();

			TempData["SaveSuccess"] = "Post kaydedildi.";
			return RedirectToAction("Detail", new { id = postId });
		}

		[Authorize]
		public ActionResult Saved()
		{
			int userId = (int)Session["UserId"];

			var vm = new PostDetailViewModel();

			vm.Posts = db.SavedPosts
				.Where(s => s.UserId == userId)
				.Select(s => s.Post)
				.OrderByDescending(p => p.CreatedAt)
				.ToList();

			vm.RecentPosts = db.Posts
				.OrderByDescending(p => p.CreatedAt)
				.Take(3)
				.ToList();

			return View("Index", vm);
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