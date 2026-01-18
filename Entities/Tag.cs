using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlogProject.Entities
{
	public class Tag : BaseEntity
	{
		[Required]
		[StringLength(50)]
		public string Name { get; set; }
		public virtual ICollection<PostTag> PostTags { get; set; }

		public Tag()
		{
			PostTags = new List<PostTag>();
		}
	}
}