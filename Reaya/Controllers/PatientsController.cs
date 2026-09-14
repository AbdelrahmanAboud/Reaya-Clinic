using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reaya.Data;
using Reaya.Models;

namespace Reaya.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class PatientsController : Controller
    {
        AppDbContext context = new AppDbContext();

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _webHostEnvironment;

        public PatientsController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            Microsoft.AspNetCore.Hosting.IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index(string? search, string? sortBy, string? sortOrder)
        {
            // Sidebar data
            ViewData["SidebarType"] = "admin";
            ViewData["UserName"] = "Rana Kamal";
            ViewData["UserRole"] = "Front desk admin";

            IQueryable<Patient> patients = context.Patients;

            if (!string.IsNullOrEmpty(search))
            {
                patients = patients.Where(p =>
                    p.Name.Contains(search) ||
                    p.PhoneNo.Contains(search) ||
                    p.Email.Contains(search));
            }

            // Sorting logic
            if (!string.IsNullOrEmpty(sortBy))
            {
                bool isAscending = sortOrder != "desc";

                switch (sortBy.ToLower())
                {
                    case "name":
                        patients = isAscending ? patients.OrderBy(p => p.Name) : patients.OrderByDescending(p => p.Name);
                        break;
                    case "phone":
                        patients = isAscending ? patients.OrderBy(p => p.PhoneNo) : patients.OrderByDescending(p => p.PhoneNo);
                        break;
                    case "email":
                        patients = isAscending ? patients.OrderBy(p => p.Email) : patients.OrderByDescending(p => p.Email);
                        break;
                    case "blood":
                        patients = isAscending ? patients.OrderBy(p => p.BloodType) : patients.OrderByDescending(p => p.BloodType);
                        break;
                    case "gender":
                        patients = isAscending ? patients.OrderBy(p => p.Gender) : patients.OrderByDescending(p => p.Gender);
                        break;
                    case "dob":
                        patients = isAscending ? patients.OrderBy(p => p.DateOfBirth) : patients.OrderByDescending(p => p.DateOfBirth);
                        break;
                }

                // Pass current sort state to view
                ViewData["CurrentSort"] = sortBy;
                ViewData["CurrentOrder"] = sortOrder;
            }

            return View(patients.ToList());
        }

        public IActionResult Create()
        {
            // Sidebar data
            ViewData["SidebarType"] = "admin";
            ViewData["UserName"] = "Rana Kamal";
            ViewData["UserRole"] = "Front desk admin";
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CurrentCreate(Patient patient)
        {
            string password = Request.Form["password"].ToString();

            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(string.Empty, "Password is required.");
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = patient.Email,
                    Email = patient.Email,
                    FullName = patient.Name,
                    PhoneNumber = patient.PhoneNo,
                    EmailConfirmed = true
                };

                var result = _userManager.CreateAsync(user, password).GetAwaiter().GetResult();

                if (result.Succeeded)
                {
                    if (!_roleManager.RoleExistsAsync("Patient").GetAwaiter().GetResult())
                    {
                        _roleManager.CreateAsync(new IdentityRole("Patient")).GetAwaiter().GetResult();
                    }

                    _userManager.AddToRoleAsync(user, "Patient").GetAwaiter().GetResult();

                    patient.UserId = user.Id;

                    if (patient.ImageFile == null)
                    {
                        patient.ImagePath = "\\images\\user_default.jpg";
                    }
                    else
                    {
                        Guid imageGuid = Guid.NewGuid();
                        string imageExtension = System.IO.Path.GetExtension(patient.ImageFile.FileName);
                        string imageNewName = imageGuid + imageExtension;
                        patient.ImagePath = "\\images\\" + imageNewName;
                        string imageFullPath = _webHostEnvironment.WebRootPath + patient.ImagePath;
                        using (var imageFileStream = new System.IO.FileStream(imageFullPath, System.IO.FileMode.Create))
                        {
                            patient.ImageFile.CopyTo(imageFileStream);
                        }
                    }

                    context.Patients.Add(patient);
                    context.SaveChanges();

                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View("Create", patient);
        }

        public IActionResult Edit(int id)
        {
            // Sidebar data
            ViewData["SidebarType"] = "admin";
            ViewData["UserName"] = "Rana Kamal";
            ViewData["UserRole"] = "Front desk admin";
            
            var patient = context.Patients.Find(id);
            if (patient == null)
            {
                return NotFound();
            }
            return View(patient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CurrentEdit(Patient patient)
        {
            if (ModelState.IsValid)
            {
                var existingPatient = context.Patients.AsNoTracking().FirstOrDefault(p => p.Id == patient.Id);
                if (existingPatient == null)
                {
                    return NotFound();
                }

                patient.UserId = existingPatient.UserId;

                if (patient.ImageFile != null)
                {
                    Guid imageGuid = Guid.NewGuid();
                    string imageExtension = System.IO.Path.GetExtension(patient.ImageFile.FileName);
                    string imageNewName = imageGuid + imageExtension;
                    patient.ImagePath = "\\images\\" + imageNewName;
                    string imageFullPath = _webHostEnvironment.WebRootPath + patient.ImagePath;
                    using (var imageFileStream = new System.IO.FileStream(imageFullPath, System.IO.FileMode.Create))
                    {
                        patient.ImageFile.CopyTo(imageFileStream);
                    }

                    // Delete old image if it's not the default one
                    if (!string.IsNullOrEmpty(existingPatient.ImagePath) && existingPatient.ImagePath != "\\images\\user_default.jpg")
                    {
                        string oldImagePath = _webHostEnvironment.WebRootPath + existingPatient.ImagePath;
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                }
                else
                {
                    // keep old image
                    patient.ImagePath = existingPatient.ImagePath;
                }

                context.Patients.Update(patient);
                context.SaveChanges();

                return RedirectToAction("Index");
            }

            return View("Edit", patient);
        }

        public IActionResult Delete(int id)
        {
            // Sidebar data
            ViewData["SidebarType"] = "admin";
            ViewData["UserName"] = "Rana Kamal";
            ViewData["UserRole"] = "Front desk admin";
            
            var patient = context.Patients.Find(id);
            if (patient == null)
            {
                return NotFound();
            }
            return View(patient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CurrentDelete(int id)
        {
            var patient = context.Patients.Find(id);
            if (patient == null)
            {
                return NotFound();
            }

            string? userId = patient.UserId;

            var appointments = context.Appointments.Where(a => a.PatientId == id).ToList();
            if (appointments.Any())
            {
                context.Appointments.RemoveRange(appointments);
            }

            var medicalRecords = context.MedicalRecords.Where(mr => mr.Appointment.PatientId == id).ToList();
            if (medicalRecords.Any())
            {
                context.MedicalRecords.RemoveRange(medicalRecords);
            }

            if (!string.IsNullOrEmpty(patient.ImagePath) && patient.ImagePath != "\\images\\user_default.jpg")
            {
                string oldImagePath = _webHostEnvironment.WebRootPath + patient.ImagePath;
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            context.Patients.Remove(patient);
            context.SaveChanges();

            if (!string.IsNullOrEmpty(userId))
            {
                var user = _userManager.FindByIdAsync(userId).GetAwaiter().GetResult();
                if (user != null)
                {
                    _userManager.DeleteAsync(user).GetAwaiter().GetResult();
                }
            }

            return RedirectToAction("Index");
        }

        public IActionResult Details(int id)
        {
            // Sidebar data
            ViewData["SidebarType"] = "admin";
            ViewData["UserName"] = "Rana Kamal";
            ViewData["UserRole"] = "Front desk admin";
            
            var patient = context.Patients.Find(id);
            if (patient == null)
            {
                return NotFound();
            }
            return View(patient);
        }
    }
}