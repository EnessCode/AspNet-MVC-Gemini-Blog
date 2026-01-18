using System;
using System.ComponentModel.DataAnnotations;

namespace BlogProject.Entities
{
	public class Comment : BaseEntity
	{
		[Required]
		[StringLength(1000, ErrorMessage = "Yorum 1000 karakteri geçemez.")]
		public string Text { get; set; }
		public DateTime CreatedAt { get; set; }
		public int PostId { get; set; }
		public virtual Post Post { get; set; }
		public int UserId { get; set; }
		public virtual User User { get; set; }

		public Comment()
		{
			CreatedAt = DateTime.UtcNow;
		}
	}
}