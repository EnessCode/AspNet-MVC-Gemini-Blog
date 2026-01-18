using System.Linq;
using System.Web.Mvc;
using BlogProject.Data;

namespace BlogProject.Controllers
{
	public class DefaultController : Controller
	{
		Context db = new Context();

		// Ana sayfa
		public ActionResult Index()
		{
			// Son eklenen 8 post
			var posts = db.Posts
						 .OrderByDescending(p => p.CreatedAt)
						 .Take(8)
						 .ToList();
			return View(posts);
		}

		// Partial1: Slider için son 5 post
		public PartialViewResult Partial1()
		{
			var sliderPosts = db.Posts
							   .OrderByDescending(p => p.CreatedAt)
							   .Take(2)
							   .ToList();
			return PartialView("Partial1", sliderPosts);
		}

		// Partial2: Öne çıkan post (örnek ID 7)
		public PartialViewResult Partial2()
		{
			var featured = db.Posts
							.Where(p => p.Id == 7)
							.ToList();
			return PartialView("Partial2", featured);
		}

		// Partial3: Son eklenen 10 post
		public PartialViewResult Partial3()
		{
			var recentPosts = db.Posts
							   .OrderByDescending(p => p.CreatedAt)
							   .Take(10)
							   .ToList();
			return PartialView("Partial3", recentPosts);
		}

		// Partial4: İlk 3 post (örnek kategori veya içerik için)
		public PartialViewResult Partial4()
		{
			var firstPosts = db.Posts
							  .OrderBy(p => p.CreatedAt)
							  .Take(3)
							  .ToList();
			return PartialView("Partial4", firstPosts);
		}

		// Partial5: Popüler 3 post (beğeni sayısına göre)
		public PartialViewResult Partial5()
		{
			var popularPosts = db.Posts
								.OrderByDescending(p => p.Likes.Count)
								.Take(3)
								.ToList();
			return PartialView("Partial5", popularPosts);
		}

		public ActionResult About()
		{
			var aboutData = db.Abouts.FirstOrDefault();

			if (aboutData == null)
			{
				aboutData = new Entities.About
				{
					Title = "Henüz İçerik Girilmedi",
					Content = "Yönetim panelinden hakkımızda yazısını güncelleyebilirsiniz.",
					ImageUrl = "/Content/images/default-about.jpg"
				};
			}

			return View(aboutData);
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
