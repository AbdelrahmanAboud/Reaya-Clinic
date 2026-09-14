using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Reaya.Models
{
    public enum AppointmentStatus
    {
        Pending,
        Confirmed,
        Completed,
        Cancelled
    }

    public class Appointment
    {
        public int Id { get; set; }

        [Display(Name = "Medical Notes")]
        public string? Notes { get; set; }

        [Required]
        [Display(Name = "Appointment Date & Time")]
        public DateTime AppointmentDate { get; set; }

        [Display(Name = "Appointment Status")]
        public AppointmentStatus Status { get; set; }

        // --- Registered patient (optional) ---
        public int? PatientId { get; set; }
        [ValidateNever]
        public Patient? Patient { get; set; }

        // --- Walk-in / unregistered patient ---
        [Display(Name = "Patient Name")]
        public string? GuestPatientName { get; set; }

        [Display(Name = "Phone Number")]
        public string? GuestPatientPhone { get; set; }

        [Required]
        public int DoctorId { get; set; }
        [ValidateNever]
        public Doctor? Doctor { get; set; }

        // Medical records for this appointment
        public ICollection<MedicalRecord>? MedicalRecords { get; set; }
    }
}
