using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlogProject.Entities
{
	public class Role : BaseEntity
	{
		[Required]
		[StringLength(50)]
		public string Name { get; set; }
		public virtual ICollection<User> Users { get; set; }

		public Role()
		{
			Users = new List<User>();
		}
	}
}