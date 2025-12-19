using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterApp.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required]
        public string ServiceName { get; set; } = string.Empty;

        public int DurationInMinutes { get; set; }

        [Range(0, 10000)]
        public decimal Price { get; set; }

        public int FitnessCenterId { get; set; }
        public FitnessCenter? FitnessCenter { get; set; }   

        public List<Appointment>? Appointments { get; set; } = new();
        public List<Trainer>? Trainers { get; set; } = new();    
    }
}
