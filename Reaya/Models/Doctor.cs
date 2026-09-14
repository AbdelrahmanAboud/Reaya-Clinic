using System.ComponentModel.DataAnnotations;
using Reaya.Data;

namespace Reaya.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        
         [Display(Name = "Full Name")] 
        public string Name { get; set; }
        public string Specialization { get; set; } 
        [Display(Name = "Phone Number")] 
        public string Phone { get; set; }
        [Display(Name = "Email Address")] 
        public string Email { get; set; }
        [Display(Name = "Consultation Fee")] 
        public decimal ConsultationFee { get; set; }

        [Display(Name = "Short biography")] 
        public string Biography { get; set; }

        [Display(Name = "Years of Experience")] 
        public string yearsExperience { get; set; }

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public string? ImagePath { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public Microsoft.AspNetCore.Http.IFormFile? ImageFile { get; set; }

    }
}