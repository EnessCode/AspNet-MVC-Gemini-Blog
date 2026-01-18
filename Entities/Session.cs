using System;

namespace BlogProject.Entities
{
	public class Session : BaseEntity
	{
		public int UserId { get; set; }
		public virtual User User { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? EndedAt { get; set; }

		public Session()
		{
			CreatedAt = DateTime.UtcNow;
		}
	}
}