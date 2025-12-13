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
        public IActionResult Index()
        {
            return View(new AiFitnessViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(AiFitnessViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var apiKey = _config["Gemini:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                model.AiResponse = "Gemini API anahtarı bulunamadı. appsettings.Development.json içine ekle.";
                return View(model);
            }

            // Prompt
            var prompt = $"""
            Kullanıcı bilgileri:
            Boy: {model.Height} cm
            Kilo: {model.Weight} kg
            Hedef: {model.Goal}

            Buna göre Türkçe, sade ve maddeler halinde:
            1) Haftalık egzersiz planı (gün gün)
            2) Diyet/kalori önerileri
            3) Dikkat edilmesi gerekenler
            """;

            // Gemini generateContent request gövdesi
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var modelName = "gemini-2.5-flash";

            var url =
                $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={apiKey}";

            using var client = new HttpClient();
            using var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage resp;
            try
            {
                resp = await client.PostAsync(url, content);
            }
            catch
            {
                model.AiResponse = "Gemini servisine bağlanılamadı (internet/SSL/proxy kontrol et).";
                return View(model);
            }

            var respText = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                // Hata mesajını ekrana bas
                model.AiResponse = $"Gemini Hatası ({(int)resp.StatusCode}): {respText}";
                return View(model);
            }

            try
            {
                using var doc = JsonDocument.Parse(respText);

                var root = doc.RootElement;

                var text =
                    root.GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();

                model.AiResponse = string.IsNullOrWhiteSpace(text)
                    ? "Gemini boş yanıt döndü."
                    : text;
            }
            catch
            {
                model.AiResponse = "Gemini cevabı parse edilemedi. Ham cevap: " + respText;
            }

            return View(model);
        }
    }
}
