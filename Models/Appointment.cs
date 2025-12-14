using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace FitnessCenterApp.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        public bool IsApproved { get; set; } = false;

        public int MemberId { get; set; }
        public Member? Member { get; set; }  

        public int TrainerId { get; set; }
        public Trainer? Trainer { get; set; } 

        public int ServiceId { get; set; }
        public Service? Service { get; set; }
        public string? UserId { get; set; }
        public IdentityUser? User { get; set; }

    }
}
