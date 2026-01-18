using System;

namespace BlogProject.Entities
{
	public class SavedPost : BaseEntity
	{
		public int UserId { get; set; }
		public virtual User User { get; set; }
		public int PostId { get; set; }
		public virtual Post Post { get; set; }
		public DateTime CreatedAt { get; set; }

		public SavedPost()
		{
			CreatedAt = DateTime.UtcNow;
		}
	}
}