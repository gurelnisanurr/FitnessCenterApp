using Microsoft.AspNetCore.Mvc.Rendering;
using FitnessCenterApp.Models;

public class AppointmentCreateVM
{
    public Appointment Appointment { get; set; } = new();

    public List<SelectListItem> Members { get; set; } = new();
    public List<SelectListItem> Trainers { get; set; } = new();
    public List<SelectListItem> Services { get; set; } = new();
    public List<SelectListItem> FitnessCenters { get; set; } = new();
}
