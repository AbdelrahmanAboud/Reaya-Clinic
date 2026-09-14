using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reaya.Data;
using Reaya.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Reaya.Controllers
{
    [Authorize(Policy = "PatientOnly")]
    public class PatientPortalController : Controller
    {
        AppDbContext context = new AppDbContext();

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PatientPortalController(UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult MyAppointments()
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

            var patient = context.Patients.FirstOrDefault(p => p.UserId == user.Id);
            if (patient == null)
            {
                // User is authenticated but not a patient
                return RedirectToAction("Index", "Home");
            }

            // Sidebar data
            ViewData["SidebarType"] = "patient";
            ViewData["UserName"] = patient?.Name ?? "Patient";
            ViewData["UserRole"] = "Patient";

            var appointments = context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == patient.Id)
                .OrderByDescending(a => a.AppointmentDate)
                .ToList();

            var upcomingAppointments = appointments.Where(a => a.AppointmentDate >= DateTime.Now).ToList();
            var pastAppointments = appointments.Where(a => a.AppointmentDate < DateTime.Now).ToList();

            ViewBag.UpcomingAppointments = upcomingAppointments;
            ViewBag.PastAppointments = pastAppointments;
            ViewBag.Patient = patient;

            return View();
        }

        public IActionResult MyMedicalHistory()
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

            var patient = context.Patients.FirstOrDefault(p => p.UserId == user.Id);
            if (patient == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Sidebar data
            ViewData["SidebarType"] = "patient";
            ViewData["UserName"] = patient?.Name ?? "Patient";
            ViewData["UserRole"] = "Patient";

            var medicalRecords = context.MedicalRecords
                .Include(mr => mr.Appointment)
                .ThenInclude(a => a.Doctor)
                .Where(mr => mr.Appointment.PatientId == patient.Id)
                .OrderByDescending(mr => mr.CreatedAt)
                .ToList();

            ViewBag.Patient = patient;

            return View(medicalRecords);
        }

        public IActionResult BookAppointment()
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

            var patient = context.Patients.FirstOrDefault(p => p.UserId == user.Id);
            if (patient == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Sidebar data
            ViewData["SidebarType"] = "patient";
            ViewData["UserName"] = patient?.Name ?? "Patient";
            ViewData["UserRole"] = "Patient";

            ViewBag.Specializations = context.Doctors
                .Select(d => d.Specialization)
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct()
                .ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveAppointment(Appointment appointment, string appointmentDate, string appointmentTime)
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

            var patient = context.Patients.FirstOrDefault(p => p.UserId == user.Id);
            if (patient == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Sidebar data
            ViewData["SidebarType"] = "patient";
            ViewData["UserName"] = patient?.Name ?? "Patient";
            ViewData["UserRole"] = "Patient";

            if (ModelState.IsValid)
            {
                // Combine date and time
                if (DateTime.TryParse($"{appointmentDate} {appointmentTime}", out DateTime combinedDateTime))
                {
                    appointment.AppointmentDate = combinedDateTime;
                }
                else
                {
                    appointment.AppointmentDate = DateTime.Now.AddDays(1);
                }

                appointment.PatientId = patient.Id;
                appointment.Status = AppointmentStatus.Pending;
                
                context.Appointments.Add(appointment);
                context.SaveChanges();

                return RedirectToAction("MyAppointments");
            }

            // Sidebar data for error case
            ViewData["SidebarType"] = "patient";
            ViewData["UserName"] = patient?.Name ?? "Patient";
            ViewData["UserRole"] = "Patient";

            ViewBag.Specializations = context.Doctors
                .Select(d => d.Specialization)
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct()
                .ToList();

            return View("BookAppointment", appointment);
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

            var patient = context.Patients.FirstOrDefault(p => p.UserId == user.Id);
            if (patient == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Sidebar data
            ViewData["SidebarType"] = "patient";
            ViewData["UserName"] = patient?.Name ?? "Patient";
            ViewData["UserRole"] = "Patient";

            return View(patient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(Patient patient, IFormFile? ImageFile)
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

            var existingPatient = context.Patients.FirstOrDefault(p => p.UserId == user.Id);
            if (existingPatient == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Sidebar data
            ViewData["SidebarType"] = "patient";
            ViewData["UserName"] = existingPatient?.Name ?? "Patient";
            ViewData["UserRole"] = "Patient";

            if (ModelState.IsValid)
            {
                existingPatient.Name = patient.Name;
                existingPatient.Email = patient.Email;
                existingPatient.PhoneNo = patient.PhoneNo;
                existingPatient.Gender = patient.Gender;
                existingPatient.DateOfBirth = patient.DateOfBirth;
                existingPatient.Address = patient.Address;
                existingPatient.BloodType = patient.BloodType;

                // Handle image upload
                if (ImageFile != null)
                {
                    // Store old image path for deletion
                    string oldImagePath = existingPatient.ImagePath;

                    // Save new image
                    Guid imageGuid = Guid.NewGuid();
                    string imageExtension = System.IO.Path.GetExtension(ImageFile.FileName);
                    string imageNewName = imageGuid + imageExtension;
                    existingPatient.ImagePath = "\\images\\" + imageNewName;
                    string imageFullPath = _webHostEnvironment.WebRootPath + existingPatient.ImagePath;
                    using (var imageFileStream = new System.IO.FileStream(imageFullPath, System.IO.FileMode.Create))
                    {
                        ImageFile.CopyTo(imageFileStream);
                    }

                    // Delete old image if it's not the default one
                    if (!string.IsNullOrEmpty(oldImagePath) && oldImagePath != "\\images\\user_default.jpg")
                    {
                        string oldImageFullPath = _webHostEnvironment.WebRootPath + oldImagePath;
                        if (System.IO.File.Exists(oldImageFullPath))
                        {
                            System.IO.File.Delete(oldImageFullPath);
                        }
                    }
                }

                context.Patients.Update(existingPatient);
                context.SaveChanges();

                // Update user email if changed
                if (user.Email != patient.Email)
                {
                    user.Email = patient.Email;
                    user.UserName = patient.Email;
                    _userManager.UpdateAsync(user).GetAwaiter().GetResult();
                }

                return RedirectToAction("MyProfile");
            }

            // Sidebar data for error case
            ViewData["SidebarType"] = "patient";
            ViewData["UserName"] = existingPatient?.Name ?? "Patient";
            ViewData["UserRole"] = "Patient";

            return View("MyProfile", existingPatient);
        }

        // GET: /PatientPortal/GetDoctorsBySpecialization?specialization=Cardiology
        [HttpGet]
        public IActionResult GetDoctorsBySpecialization(string specialization)
        {
            if (string.IsNullOrWhiteSpace(specialization))
                return Json(new List<object>());

            var doctors = context.Doctors
                .Where(d => d.Specialization == specialization)
                .Select(d => new
                {
                    d.Id,
                    d.Name,
                    d.Specialization,
                    d.ConsultationFee
                })
                .ToList();

            return Json(doctors);
        }
    }
}