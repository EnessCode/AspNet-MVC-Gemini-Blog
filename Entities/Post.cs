using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlogProject.Entities
{
	public class Post : BaseEntity
	{
		[Required]
		[StringLength(150, ErrorMessage = "Başlık 150 karakteri geçemez.")]
		public string Title { get; set; }

		[Required] 
		public string Content { get; set; }

		[StringLength(500)]
		public string ImageUrl { get; set; }
		public DateTime CreatedAt { get; set; }
		public int UserId { get; set; }
		public virtual User User { get; set; }
		public int CategoryId { get; set; }
		public virtual Category Category { get; set; }

		public virtual ICollection<PostTag> PostTags { get; set; }
		public virtual ICollection<Comment> Comments { get; set; }
		public virtual ICollection<Like> Likes { get; set; }
		public virtual ICollection<SavedPost> SavedPosts { get; set; }

		public Post()
		{
			CreatedAt = DateTime.UtcNow;
			PostTags = new List<PostTag>();
			Comments = new List<Comment>();
			Likes = new List<Like>();
			SavedPosts = new List<SavedPost>();
		}
	}
}