using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterApp.Models
{
    public class FitnessCenter
    {
        public int Id { get; set; }

        [Required]
        [StringLength(80)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; }

        public string WorkingHours { get; set; }

        public List<Service> Services { get; set; } = new List<Service>();
        public List<Trainer> Trainers { get; set; } = new List<Trainer>();
    }
}
