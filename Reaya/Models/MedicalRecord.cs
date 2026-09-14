using System.ComponentModel.DataAnnotations;

namespace Reaya.Models
{
    public class MedicalRecord
    {
        public int Id { get; set; }

        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; }

        [Required(ErrorMessage = "Diagnosis is required")]
        [StringLength(500, ErrorMessage = "Diagnosis cannot be longer than 500 characters")]
        public string Diagnosis { get; set; } 
        
        [StringLength(1000, ErrorMessage = "Prescription cannot be longer than 1000 characters")]
        public string Prescription { get; set; } 
        
        [StringLength(1000, ErrorMessage = "Doctor notes cannot be longer than 1000 characters")]
        public string DoctorNotes { get; set; } 
        
        public DateTime CreatedAt { get; set; } = DateTime.Now; 
    }
}
