using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterApp.Models
{
    public class Member
    {
        public int Id { get; set; }

        [Display(Name = "Üye Adı Soyadı")]
        [Required(ErrorMessage = "{0} alanı zorunludur.")]
        public string FullName { get; set; }

        [Display(Name = "E-Posta Adresi")]
        [Required(ErrorMessage = "{0} alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; }

        [Display(Name = "Telefon Numarası")]
        public string Phone { get; set; }

        public List<Appointment>? Appointments { get; set; }
    }
}