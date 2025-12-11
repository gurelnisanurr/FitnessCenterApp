using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterApp.Models
{
    public class Trainer
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        // "Kas geliştirme, fitness" vs.
        public string SpecialtyText { get; set; } = string.Empty;

        public int FitnessCenterId { get; set; }
        public FitnessCenter? FitnessCenter { get; set; }

        // Many-to-many: Eğitmenin verdiği hizmetler
        public List<Service>? Services { get; set; } = new();

        // Eğitmenin randevuları
        public List<Appointment>? Appointments { get; set; } = new();
    }
}


