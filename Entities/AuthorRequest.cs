using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BlogProject.Entities
{
	public class AuthorRequest : BaseEntity
	{
		public int UserId { get; set; }

		[ForeignKey("UserId")]
		public virtual User User { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.Now;

		public bool IsProcessed { get; set; } = false; 
		public bool IsApproved { get; set; } = false; 
	}
}