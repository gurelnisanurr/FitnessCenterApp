using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterApp.Models
{
    public class Trainer
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string SpecialtyText { get; set; }

        // Günlük müsaitlik saatleri
        [Required]
        public TimeSpan AvailableFrom { get; set; } = new TimeSpan(9, 0, 0);

        [Required]
        public TimeSpan AvailableTo { get; set; } = new TimeSpan(18, 0, 0);

        public int FitnessCenterId { get; set; }
        public FitnessCenter? FitnessCenter { get; set; }

        public List<Service>? Services { get; set; } = new();
        public List<Appointment>? Appointments { get; set; } = new();
    }
}

