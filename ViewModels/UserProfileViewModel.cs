using BlogProject.Entities;
using System.Collections.Generic;

namespace BlogProject.ViewModels
{
	public class UserProfileViewModel
	{
		public User User { get; set; }
		public List<Comment> MyComments { get; set; }
		public List<Post> LikedPosts { get; set; }
		public List<Post> SavedPosts { get; set; }

		public UserProfileViewModel()
		{
			MyComments = new List<Comment>();
			LikedPosts = new List<Post>();
			SavedPosts = new List<Post>();
		}
	}
}