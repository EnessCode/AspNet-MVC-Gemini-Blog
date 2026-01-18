using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlogProject.ViewModels
{
	public class DashboardViewModel
	{
		public int TotalPosts { get; set; }
		public int TotalComments { get; set; }
		public int TotalUsers { get; set; }
		public int TotalLikes { get; set; }

		public List<string> CategoryNames { get; set; }
		public List<int> CategoryPostCounts { get; set; }
		public List<CommentViewModel> LastComments { get; set; }
	}
}