using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BlogProject.Helpers
{
	public class GeminiModerator
	{
		private static readonly string ApiKey = ConfigurationManager.AppSettings["GeminiApiKey"];

		private static readonly string ApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key=" + ApiKey;

		public static async Task<string> CheckContent(string text)
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

			var prompt = $@"
                Sen bir moderatörsün. Aşağıdaki metni analiz et.
                Metin: ""{text}""

                Eğer metin küfür, hakaret, aşağılama, tehdit veya nefret söylemi içeriyorsa SADECE şu kelimeyi yaz:
                RET

                Eğer metin uygunsa SADECE şu kelimeyi yaz:
                ONAY
            ";

			var requestBody = new
			{
				contents = new[]
				{
					new { parts = new[] { new { text = prompt } } }
				}
			};

			var jsonContent = JsonConvert.SerializeObject(requestBody);
			var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

			using (var client = new HttpClient())
			{
				var response = await client.PostAsync(ApiUrl, httpContent);
				var responseBody = await response.Content.ReadAsStringAsync();

				if (!response.IsSuccessStatusCode)
				{
					return "HATA: " + response.StatusCode + " - " + responseBody;
				}

				dynamic result = JsonConvert.DeserializeObject(responseBody);

				if (result?.candidates == null || result.candidates.Count == 0)
					return "HATA: Yapay zeka boş cevap döndü.";

				try
				{
					string answer = result.candidates[0].content.parts[0].text;
					return answer.Trim().ToUpper();
				}
				catch
				{
					return "HATA: Cevap okunamadı.";
				}
			}
		}
	}
}