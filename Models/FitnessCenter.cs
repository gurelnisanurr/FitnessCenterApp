using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterApp.Models
{
    public class FitnessCenter
    {
        public int Id { get; set; }

        [Display(Name = "Salon Adı")]
        [Required(ErrorMessage = "{0} alanı zorunludur.")]
        [StringLength(80, ErrorMessage = "{0} en fazla {1} karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Adres")]
        [Required(ErrorMessage = "{0} alanı zorunludur.")]
        public string Address { get; set; }

        [Display(Name = "Çalışma Saatleri")]
        public string WorkingHours { get; set; }

        public List<Service> Services { get; set; } = new();
        public List<Trainer> Trainers { get; set; } = new();
        public List<Appointment> Appointments { get; set; } = new();
    }
}
