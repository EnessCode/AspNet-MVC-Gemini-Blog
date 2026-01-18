using System;

namespace BlogProject.ViewModels
{
	public class CommentViewModel
	{
		public int Id { get; set; }
		public string Username { get; set; }
		public string Text { get; set; }
		public int PostId { get; set; }
		public string PostTitle { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}