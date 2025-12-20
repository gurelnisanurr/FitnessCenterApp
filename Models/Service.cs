using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterApp.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Display(Name = "Hizmet Adı")]
        [Required(ErrorMessage = "{0} alanı zorunludur.")]
        public string ServiceName { get; set; } = string.Empty;

        [Display(Name = "Süre (Dakika)")]
        [Required(ErrorMessage = "{0} alanı zorunludur.")]
        public int DurationInMinutes { get; set; }

        [Display(Name = "Ücret (TL)")]
        [Required(ErrorMessage = "{0} alanı zorunludur.")]
        [Range(0, 10000, ErrorMessage = "{0} 0 ile {2} arasında olmalıdır.")]
        public decimal Price { get; set; }

        [Display(Name = "Spor Salonu")]
        public int FitnessCenterId { get; set; }
        public FitnessCenter? FitnessCenter { get; set; }

        public List<Appointment>? Appointments { get; set; } = new();
        public List<Trainer>? Trainers { get; set; } = new();
    }
}