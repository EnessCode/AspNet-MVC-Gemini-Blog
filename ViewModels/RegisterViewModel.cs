using System.ComponentModel.DataAnnotations;

namespace BlogProject.ViewModels
{
	public class RegisterViewModel
	{
		[Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
		[StringLength(50, ErrorMessage = "Kullanıcı adı en fazla 50 karakter olabilir.")]
		public string Username { get; set; }

		[Required(ErrorMessage = "E-posta zorunludur.")]
		[EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
		[StringLength(100, ErrorMessage = "E-posta en fazla 100 karakter olabilir.")]
		public string Email { get; set; }

		[Required(ErrorMessage = "Şifre zorunludur.")]
		[MinLength(5, ErrorMessage = "Şifre en az 5 karakter olmalıdır.")]
		public string Password { get; set; }

		[Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
		[Compare("Password", ErrorMessage = "Şifreler uyuşmuyor.")]
		public string RePassword { get; set; }
	}
}