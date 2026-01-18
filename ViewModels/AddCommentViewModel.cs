using System.ComponentModel.DataAnnotations;

namespace BlogProject.ViewModels
{
	public class AddCommentViewModel
	{
		public int PostId { get; set; }

		public string Username { get; set; }

		[Required(ErrorMessage = "Yorum içeriği boş olamaz.")]
		public string Text { get; set; }
	}
}