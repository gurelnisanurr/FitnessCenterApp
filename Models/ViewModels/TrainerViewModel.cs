using System.Collections.Generic;
using FitnessCenterApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitnessCenterApp.Models.ViewModels
{
    public class TrainerViewModel
    {
        public Trainer Trainer { get; set; }

        public List<SelectListItem>? Services { get; set; }

        // Checkbox ile seçilecek olan Service Id listesi
        public List<int> SelectedServiceIds { get; set; } = new();
    }
}
