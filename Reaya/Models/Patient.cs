using Reaya.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Reaya.Models
{
    public class Patient
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Phone number is required")]
        [StringLength(20, ErrorMessage = "Phone number cannot be longer than 20 characters")]
        public string PhoneNo { get; set; }
        
        [Required(ErrorMessage = "Gender is required")]
        [StringLength(10, ErrorMessage = "Gender cannot be longer than 10 characters")]
        public string Gender { get; set; } 
        
        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime DateOfBirth { get; set; }
        
        [StringLength(200, ErrorMessage = "Address cannot be longer than 200 characters")]
        public string Address { get; set; }
        
        [StringLength(5, ErrorMessage = "Blood type cannot be longer than 5 characters")]
        public string BloodType { get; set; }

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public string? ImagePath { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public Microsoft.AspNetCore.Http.IFormFile? ImageFile { get; set; }
    }
}
