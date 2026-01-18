using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlogProject.Entities
{
	public class Category : BaseEntity
	{
		[Required]
		[StringLength(50)]
		public string Title { get; set; }
		public virtual ICollection<Post> Posts { get; set; }

		public Category()
		{
			Posts = new List<Post>();
		}
	}
}