using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FitnessCenterApp.Models.ViewModels;

namespace FitnessCenterApp.Controllers
{
    [Authorize]
    public class AiFitnessController : Controller
    {
        private readonly IConfiguration _config;

        public AiFitnessController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public IActionResult Index() => View(new AiFitnessViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(AiFitnessViewModel model, IFormFile? UserPhoto)
        {
            if (!ModelState.IsValid) return View(model);

            var apiKey = _config["Gemini:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                model.AiResponse = "Hata: API Key bulunamadı.";
                return View(model);
            }

            string base64Image = "";
            if (UserPhoto != null && UserPhoto.Length > 0)
            {
                using var ms = new MemoryStream();
                await UserPhoto.CopyToAsync(ms);
                base64Image = Convert.ToBase64String(ms.ToArray());
            }

            // Prompt: AI'dan anatomik analiz istiyoruz
            var prompt = $"""
            Kullanıcı Bilgileri: Boy {model.Height}cm, Kilo {model.Weight}kg, Hedef: {model.Goal}.
            Lütfen Türkçe olarak: 
            1. Mevcut fiziksel durumu analiz et.
            2. Bu hedefe ulaşmak için gereken temel önerileri ver.
            3. Gelecekteki hali için profesyonel bir betimleme yap.
            """;

            var requestBody = new
            {
                contents = new[] {
                    new {
                        parts = string.IsNullOrEmpty(base64Image)
                            ? (object[])new[] { new { text = prompt } }
                            : new object[] {
                                new { text = prompt },
                                new { inline_data = new { mime_type = "image/jpeg", data = base64Image } }
                            }
                    }
                }
            };

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

            using var client = new HttpClient();
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var resp = await client.PostAsync(url, content);
                var respText = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    model.AiResponse = $"Gemini Hatası ({(int)resp.StatusCode}): {respText}";
                    return View(model);
                }

                using var doc = JsonDocument.Parse(respText);
                model.AiResponse = doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();

                if (!string.IsNullOrEmpty(base64Image)) ViewBag.UploadedPhoto = $"data:image/jpeg;base64,{base64Image}";

                ViewBag.GoalImage = model.Goal switch
                {
                    "Kilo vermek" => "/images/zayif.png",
                    "Aşırı Kilo Vermek" => "/images/asiri_zayif.png",
                    "Kas kazanmak" => "/images/kasli.png",
                    "Aşırı Kas Yapmak" => "/images/asiri_kasli.png",
                    "Formda kalmak" => "/images/formda.png",
                    _ => "/images/formda.png"
                };
            }
            catch (Exception ex)
            {
                model.AiResponse = "Bağlantı Hatası: " + ex.Message;
            }
            return View(model);
        }
    }
}