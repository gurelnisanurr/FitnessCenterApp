using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterApp.Models
{
    public class Trainer
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        public string SpecialtyText { get; set; } // "Kas geliştirme, fitness" vs.

        public int FitnessCenterId { get; set; }
        public FitnessCenter FitnessCenter { get; set; }

        public List<Service> Services { get; set; }
        public List<Appointment> Appointments { get; set; }
    }
}

