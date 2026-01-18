using System.Net;
using System.Net.Mail;

namespace BlogProject.Helpers
{
	public class MailHelper
	{
		public static void SendResetEmail(string toEmail, string resetLink)
		{
			var senderEmail = "1test01010101@gmail.com";
			var senderPassword = "pkqf ejxn pwut dolw";

			var smtp = new SmtpClient
			{
				Host = "smtp.gmail.com",
				Port = 587,
				EnableSsl = true,
				DeliveryMethod = SmtpDeliveryMethod.Network,
				UseDefaultCredentials = false,
				Credentials = new NetworkCredential(senderEmail, senderPassword)
			};

			using (var message = new MailMessage(senderEmail, toEmail))
			{
				message.Subject = "Şifre Sıfırlama İsteği - BlogProject";
				message.Body = $@"<h3>Şifre Sıfırlama</h3>
                                  <p>Şifrenizi sıfırlamak için lütfen aşağıdaki linke tıklayınız:</p>
                                  <p><a href='{resetLink}'>Şifremi Sıfırla</a></p>
                                  <p>Bu link 1 saat süreyle geçerlidir.</p>";
				message.IsBodyHtml = true;
				smtp.Send(message);
			}
		}
	}
}