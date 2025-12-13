namespace FitnessCenterApp.Models.ViewModels
{
    public class AiFitnessViewModel
    {
        public int Height { get; set; }   // cm
        public int Weight { get; set; }   // kg
        public string Goal { get; set; } = string.Empty;

        public string? AiResponse { get; set; }
    }
}
