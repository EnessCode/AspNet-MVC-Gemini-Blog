using System;
using System.ComponentModel.DataAnnotations;

namespace BlogProject.Entities
{
	public class Notification : BaseEntity
	{
		[Required]
		[StringLength(50)]
		public string Type { get; set; } 

		[Required]
		[StringLength(250)]
		public string Message { get; set; }
		public DateTime CreatedAt { get; set; }
		public bool IsRead { get; set; }
		public int? RelatedId { get; set; } 
		public int? UserId { get; set; }
		public virtual User User { get; set; }

		public Notification()
		{
			CreatedAt = DateTime.UtcNow;
			IsRead = false;
		}
	}
}