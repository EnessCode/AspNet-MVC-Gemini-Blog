using BlogProject.Entities;
using System.Collections.Generic;

namespace BlogProject.ViewModels
{
	public class PostDetailViewModel
	{
		public List<Post> Posts { get; set; }
		public List<Comment> Comments { get; set; }
		public List<Post> RecentPosts { get; set; }

		public PostDetailViewModel()
		{
			Posts = new List<Post>();
			Comments = new List<Comment>();
			RecentPosts = new List<Post>();
		}
	}
}