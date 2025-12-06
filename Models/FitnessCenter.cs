using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterApp.Models
{
    public class FitnessCenter
    {
        public int Id { get; set; }

        [Required]
        [StringLength(80)]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        public string WorkingHours { get; set; } // ör: 08:00 - 22:00

        public List<Service> Services { get; set; }
        public List<Trainer> Trainers { get; set; }
    }
}
