using System.ComponentModel.DataAnnotations;

namespace BlogProject.Entities
{
	public class About : BaseEntity
	{
		[StringLength(100)]
		[Required]
		public string Title { get; set; } 

		[Required]
		public string Content { get; set; } 

		[StringLength(500)]
		public string ImageUrl { get; set; } 

		[StringLength(500)]
		public string MapLocation { get; set; } 
	}
}