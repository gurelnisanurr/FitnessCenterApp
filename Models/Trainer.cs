using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterApp.Models
{
    public class Trainer
    {
        public int Id { get; set; }

        [Display(Name = "Ad Soyad")]
        [Required(ErrorMessage = "{0} alanı zorunludur.")]
        public string FullName { get; set; }

        [Display(Name = "Uzmanlık Alanı")]
        [Required(ErrorMessage = "{0} alanı zorunludur.")]
        public string SpecialtyText { get; set; }

        [Display(Name = "Müsaitlik Başlangıç")]
        [Required(ErrorMessage = "{0} alanı zorunludur.")]
        public TimeSpan AvailableFrom { get; set; } = new TimeSpan(9, 0, 0);

        [Display(Name = "Müsaitlik Bitiş")]
        [Required(ErrorMessage = "{0} alanı zorunludur.")]
        public TimeSpan AvailableTo { get; set; } = new TimeSpan(18, 0, 0);

        [Display(Name = "Spor Salonu")]
        public int FitnessCenterId { get; set; }
        public FitnessCenter? FitnessCenter { get; set; }

        public List<Service>? Services { get; set; } = new();
        public List<Appointment>? Appointments { get; set; } = new();
    }
}