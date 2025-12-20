using System;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterApp.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Lütfen bir üye seçiniz.")]
        [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir üye seçiniz.")]
        public int MemberId { get; set; }

        [Required(ErrorMessage = "Lütfen bir eğitmen seçiniz.")]
        [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir eğitmen seçiniz.")]
        public int TrainerId { get; set; }

        [Required(ErrorMessage = "Lütfen bir hizmet seçiniz.")]
        [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir hizmet seçiniz.")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Lütfen spor salonu şubesini seçiniz.")]
        [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir salon seçiniz.")]
        public int FitnessCenterId { get; set; }

        [Required(ErrorMessage = "Lütfen randevu tarihini ve saatini seçiniz.")]
        [DataType(DataType.DateTime)]
        public DateTime AppointmentDate { get; set; } = DateTime.Now;

        public bool IsApproved { get; set; } = false;

        // Navigation Properties
        public Member? Member { get; set; }
        public Trainer? Trainer { get; set; }
        public Service? Service { get; set; }
        public FitnessCenter? FitnessCenter { get; set; }
    }
}