using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reaya.Data;
using Reaya.Models;

namespace Reaya.Controllers
{
    [Authorize(Policy = "DoctorOnly")]
    public class DoctorDashboardController : Controller
    {
        AppDbContext context = new AppDbContext();

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _webHostEnvironment;

        public DoctorDashboardController(UserManager<ApplicationUser> userManager, Microsoft.AspNetCore.Hosting.IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult MyDay()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var user = _userManager.GetUserAsync(User).GetAwaiter().GetResult();
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var doctor = context.Doctors.FirstOrDefault(d => d.UserId == user.Id);
            if (doctor == null)
            {
                // User is authenticated but not a doctor
                return RedirectToAction("Index", "Home");
            }

            // Sidebar data
            ViewData["SidebarType"] = "doctor";
            ViewData["UserName"] = doctor?.Name ?? "Doctor";
            ViewData["UserRole"] = doctor?.Specialization ?? "Specialist";

            var today = DateTime.Today;
            var todaysAppointments = context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctor.Id && a.AppointmentDate.Date == today)
                .OrderBy(a => a.AppointmentDate)
                .ToList();

            var totalPatients = context.Appointments
                .Where(a => a.DoctorId == doctor.Id)
                .Select(a => a.PatientId)
                .Distinct()
                .Count();

            var completedAppointments = context.Appointments
                .Count(a => a.DoctorId == doctor.Id && a.Status == AppointmentStatus.Completed && a.AppointmentDate.Date == today);

            var pendingAppointments = context.Appointments
                .Count(a => a.DoctorId == doctor.Id && a.Status == AppointmentStatus.Pending && a.AppointmentDate.Date == today);

            var nextAppointment = context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctor.Id && a.AppointmentDate > DateTime.Now)
                .OrderBy(a => a.AppointmentDate)
                .FirstOrDefault();

            ViewBag.Doctor = doctor;
            ViewBag.TotalPatients = totalPatients;
            ViewBag.CompletedAppointments = completedAppointments;
            ViewBag.PendingAppointments = pendingAppointments;
            ViewBag.NextAppointment = nextAppointment;

            return View(todaysAppointments);
        }

        public IActionResult Consultation(int appointmentId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var user = _userManager.GetUserAsync(User).GetAwaiter().GetResult();
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var doctor = context.Doctors.FirstOrDefault(d => d.UserId == user.Id);
            if (doctor == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Sidebar data
            ViewData["SidebarType"] = "doctor";
            ViewData["UserName"] = doctor?.Name ?? "Doctor";
            ViewData["UserRole"] = doctor?.Specialization ?? "Specialist";

            var appointment = context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.MedicalRecords)
                .FirstOrDefault(a => a.Id == appointmentId && a.DoctorId == doctor.Id);

            if (appointment == null)
            {
                return NotFound();
            }

            // Get patient's medical history
            var patientMedicalHistory = context.MedicalRecords
                .Include(mr => mr.Appointment)
                .ThenInclude(a => a.Doctor)
                .Where(mr => mr.Appointment.PatientId == appointment.PatientId)
                .OrderByDescending(mr => mr.CreatedAt)
                .ToList();

            ViewBag.PatientMedicalHistory = patientMedicalHistory;

            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveConsultation(MedicalRecord medicalRecord, int appointmentId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var user = _userManager.GetUserAsync(User).GetAwaiter().GetResult();
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var doctor = context.Doctors.FirstOrDefault(d => d.UserId == user.Id);
            if (doctor == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Sidebar data
            ViewData["SidebarType"] = "doctor";
            ViewData["UserName"] = doctor?.Name ?? "Doctor";
            ViewData["UserRole"] = doctor?.Specialization ?? "Specialist";

            var appointment = context.Appointments.Find(appointmentId);
            if (appointment == null || appointment.DoctorId != doctor.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Appointment");
            if (ModelState.IsValid)
            {
                medicalRecord.AppointmentId = appointmentId;
                medicalRecord.CreatedAt = DateTime.Now;
                
                context.MedicalRecords.Add(medicalRecord);
                context.SaveChanges();

                // Update appointment status to completed
                appointment.Status = AppointmentStatus.Completed;
                context.SaveChanges();

                return RedirectToAction("MyDay");
            }

            // Reload data for view
            appointment = context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.MedicalRecords)
                .FirstOrDefault(a => a.Id == appointmentId);

            var patientMedicalHistory = context.MedicalRecords
                .Include(mr => mr.Appointment)
                .ThenInclude(a => a.Doctor)
                .Where(mr => mr.Appointment.PatientId == appointment.PatientId)
                .OrderByDescending(mr => mr.CreatedAt)
                .ToList();

            ViewBag.PatientMedicalHistory = patientMedicalHistory;

            return View("Consultation", appointment);
        }

        public IActionResult MyProfile()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var user = _userManager.GetUserAsync(User).GetAwaiter().GetResult();
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var doctor = context.Doctors.FirstOrDefault(d => d.UserId == user.Id);
            if (doctor == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Sidebar data
            ViewData["SidebarType"] = "doctor";
            ViewData["UserName"] = doctor?.Name ?? "Doctor";
            ViewData["UserRole"] = doctor?.Specialization ?? "Specialist";

            return View(doctor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(Doctor doctor, string currentPassword, string newPassword, string confirmPassword)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var user = _userManager.GetUserAsync(User).GetAwaiter().GetResult();
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var existingDoctor = context.Doctors.FirstOrDefault(d => d.UserId == user.Id);
            if (existingDoctor == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Sidebar data
            ViewData["SidebarType"] = "doctor";
            ViewData["UserName"] = existingDoctor?.Name ?? "Doctor";
            ViewData["UserRole"] = existingDoctor?.Specialization ?? "Specialist";

            // Remove validation errors for password fields if they're empty (optional password change)
            if (string.IsNullOrEmpty(currentPassword) && string.IsNullOrEmpty(newPassword) && string.IsNullOrEmpty(confirmPassword))
            {
                ModelState.Remove("currentPassword");
                ModelState.Remove("newPassword");
                ModelState.Remove("confirmPassword");
            }

            // Handle password change if provided
            if (!string.IsNullOrEmpty(newPassword) || !string.IsNullOrEmpty(confirmPassword) || !string.IsNullOrEmpty(currentPassword))
            {
                if (string.IsNullOrEmpty(currentPassword))
                {
                    ModelState.AddModelError(string.Empty, "Current password is required when changing password.");
                    return View("MyProfile", existingDoctor);
                }

                if (newPassword != confirmPassword)
                {
                    ModelState.AddModelError(string.Empty, "New password and confirmation password do not match.");
                    return View("MyProfile", existingDoctor);
                }

                // Verify current password
                var passwordCheck = _userManager.CheckPasswordAsync(user, currentPassword).GetAwaiter().GetResult();
                if (!passwordCheck)
                {
                    ModelState.AddModelError(string.Empty, "Current password is incorrect.");
                    return View("MyProfile", existingDoctor);
                }

                // Change password
                var result = _userManager.ChangePasswordAsync(user, currentPassword, newPassword).GetAwaiter().GetResult();
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View("MyProfile", existingDoctor);
                }
            }

            // Remove validation errors for fields we're not updating
            ModelState.Remove("doctor.Id");
            ModelState.Remove("doctor.UserId");
            ModelState.Remove("doctor.ImagePath");
            
            // Check if model state is valid for personal information
            if (!ModelState.IsValid)
            {
                return View("MyProfile", existingDoctor);
            }

            // Update personal information
            existingDoctor.Name = doctor.Name;
            existingDoctor.Email = doctor.Email;
            existingDoctor.Phone = doctor.Phone;
            existingDoctor.Specialization = doctor.Specialization;
            existingDoctor.ConsultationFee = doctor.ConsultationFee;
            existingDoctor.Biography = doctor.Biography;
            existingDoctor.yearsExperience = doctor.yearsExperience;

            // Handle image upload
            if (doctor.ImageFile != null)
            {
                // Get old image path before updating
                string oldImagePath = existingDoctor.ImagePath;
                
                // Delete old image if it's not the default one
                if (!string.IsNullOrEmpty(oldImagePath) && oldImagePath != "\\images\\user_default.jpg")
                {
                    string oldImageFullPath = _webHostEnvironment.WebRootPath + oldImagePath;
                    if (System.IO.File.Exists(oldImageFullPath))
                    {
                        System.IO.File.Delete(oldImageFullPath);
                    }
                }

                Guid imageGuid = Guid.NewGuid();
                string imageExtension = System.IO.Path.GetExtension(doctor.ImageFile.FileName);
                string imageNewName = imageGuid + imageExtension;
                existingDoctor.ImagePath = "\\images\\" + imageNewName;
                string imageFullPath = _webHostEnvironment.WebRootPath + existingDoctor.ImagePath;
                using (var imageFileStream = new System.IO.FileStream(imageFullPath, System.IO.FileMode.Create))
                {
                    doctor.ImageFile.CopyTo(imageFileStream);
                }
            }

            context.Doctors.Update(existingDoctor);
            context.SaveChanges();

            // Update user email if changed
            if (user.Email != doctor.Email)
            {
                user.Email = doctor.Email;
                user.UserName = doctor.Email;
                _userManager.UpdateAsync(user).GetAwaiter().GetResult();
            }

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("MyProfile");
        }
    }
}