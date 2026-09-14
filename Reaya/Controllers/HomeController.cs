using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reaya.Data;
using Reaya.Models;
using System.Diagnostics;

namespace Reaya.Controllers
{
    public class HomeController : Controller
    {
        AppDbContext context = new AppDbContext();

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public HomeController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = _userManager.GetUserAsync(User).GetAwaiter().GetResult();
                if (user != null)
                {
                    var roles = _userManager.GetRolesAsync(user).GetAwaiter().GetResult();

                    if (roles.Contains("Admin"))
                    {
                        return RedirectToAction("Overview");
                    }
                    else if (roles.Contains("Doctor"))
                    {
                        return RedirectToAction("MyDay", "DoctorDashboard");
                    }
                    else if (roles.Contains("Patient"))
                    {
                        return RedirectToAction("MyAppointments", "PatientPortal");
                    }
                }
            }
            
            ViewBag.AvailableDoctors = context.Doctors.Count();
            ViewBag.DepartmentsCount = context.Doctors.Select(d => d.Specialization).Distinct().Count();
            
            var topDoctors = context.Doctors.Take(6).ToList();
            return View(topDoctors);
        }

        [Microsoft.AspNetCore.Authorization.Authorize]
        public IActionResult SmartRedirect(string target = "home")
        {
            var user = _userManager.GetUserAsync(User).GetAwaiter().GetResult();
            if (user != null)
            {
                var roles = _userManager.GetRolesAsync(user).GetAwaiter().GetResult();

                if (roles.Contains("Admin"))
                    return RedirectToAction("Overview");

                if (roles.Contains("Doctor"))
                    return RedirectToAction("MyDay", "DoctorDashboard");

                if (roles.Contains("Patient"))
                {
                    if (target == "book")
                        return RedirectToAction("BookAppointment", "PatientPortal");
                    return RedirectToAction("MyAppointments", "PatientPortal");
                }
            }
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize(Policy = "AdminOnly")]
        public IActionResult Overview(string? search, string? sortBy, string? sortOrder)
        {
            // Get current user
            var user = _userManager.GetUserAsync(User).GetAwaiter().GetResult();

            // Sidebar data
            ViewData["SidebarType"] = "admin";
            ViewData["UserName"] = user?.FullName ?? user?.Email ?? "Admin";
            ViewData["UserRole"] = "Front desk admin";
            ViewData["GreetingName"] = user?.FullName ?? user?.Email ?? "Admin";

            var totalAppointments = context.Appointments.Count();

            if (totalAppointments == 0) totalAppointments = 1;

            var specializationLoads = context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.Doctor != null && a.Doctor.Specialization != null)
                .GroupBy(a => a.Doctor.Specialization)
                .Select(g => new {
                    Specialization = g.Key,
                    Count = g.Count(),

                    Percentage = (int)Math.Round((double)g.Count() / totalAppointments * 100)
                })
                .OrderByDescending(x => x.Percentage)
                .ToList();

            ViewBag.SpecializationLoads = specializationLoads;

            ViewBag.TotalPatients = context.Appointments.Count();
            ViewBag.AvailableDoctors = context.Doctors.Count();
            ViewBag.TodaysAppointments = context.Appointments.Count(a => a.AppointmentDate.Date == DateTime.Today);
            ViewBag.DepartmentsCount = context.Doctors.Select(d => d.Specialization).Distinct().Count();
            ViewBag.PendingAppointments = context.Appointments.Count(a => a.Status == AppointmentStatus.Pending && a.AppointmentDate.Date == DateTime.Today);

            IQueryable<Appointment> query = context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(a =>
                    (a.Patient != null && a.Patient.Name.Contains(search)) ||
                    (a.GuestPatientName != null && a.GuestPatientName.Contains(search)) ||
                    (a.Doctor != null && a.Doctor.Name.Contains(search)) ||
                    a.Status.ToString().Contains(search)
                );
            }

            // Sorting logic
            if (!string.IsNullOrEmpty(sortBy))
            {
                bool isAscending = sortOrder != "desc";

                switch (sortBy.ToLower())
                {
                    case "patient":
                        query = isAscending ? query.OrderBy(a => a.Patient != null ? a.Patient.Name : a.GuestPatientName) : query.OrderByDescending(a => a.Patient != null ? a.Patient.Name : a.GuestPatientName);
                        break;
                    case "doctor":
                        query = isAscending ? query.OrderBy(a => a.Doctor != null ? a.Doctor.Name : "") : query.OrderByDescending(a => a.Doctor != null ? a.Doctor.Name : "");
                        break;
                    case "date":
                        query = isAscending ? query.OrderBy(a => a.AppointmentDate) : query.OrderByDescending(a => a.AppointmentDate);
                        break;
                    case "status":
                        query = isAscending ? query.OrderBy(a => a.Status) : query.OrderByDescending(a => a.Status);
                        break;
                }

                // Pass current sort state to view
                ViewData["CurrentSort"] = sortBy;
                ViewData["CurrentOrder"] = sortOrder;
            }
            else
            {
                // Default sorting: by date descending
                query = query.OrderByDescending(a => a.AppointmentDate);
            }

            var appointments = query.Take(6).ToList();

            return View(appointments);
        }

        // Temporary action to assign Admin role - REMOVE IN PRODUCTION
        public IActionResult AssignAdmin(string email)
        {
            var user = _userManager.FindByEmailAsync(email).GetAwaiter().GetResult();
            if (user != null)
            {
                // Remove Patient role if exists
                _userManager.RemoveFromRoleAsync(user, "Patient").GetAwaiter().GetResult();

                // Assign Admin role
                if (!_roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
                {
                    _roleManager.CreateAsync(new IdentityRole("Admin")).GetAwaiter().GetResult();
                }

                var result = _userManager.AddToRoleAsync(user, "Admin").GetAwaiter().GetResult();
                if (result.Succeeded)
                {
                    return Content($"Admin role assigned to {email}. Patient role removed.");
                }
                else
                {
                    return Content($"Failed to assign Admin role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            return Content($"User not found: {email}");
        }

        // Temporary action to remove Admin role - REMOVE IN PRODUCTION
        public IActionResult RemoveAdmin(string email)
        {
            var user = _userManager.FindByEmailAsync(email).GetAwaiter().GetResult();
            if (user != null)
            {
                var result = _userManager.RemoveFromRoleAsync(user, "Admin").GetAwaiter().GetResult();
                if (result.Succeeded)
                {
                    return Content($"Admin role removed from {email}");
                }
                else
                {
                    return Content($"Failed to remove Admin role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            return Content($"User not found: {email}");
        }

        // Temporary action to add Admin role back - REMOVE IN PRODUCTION
        public IActionResult CreateAdminRole()
        {
            if (!_roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole("Admin")).GetAwaiter().GetResult();
                return Content("Admin role created successfully. First user to register will get Admin role.");
            }
            else
            {
                return Content("Admin role already exists.");
            }
        }

        // Temporary action to add Patient role back - REMOVE IN PRODUCTION
        public IActionResult CreatePatientRole()
        {
            if (!_roleManager.RoleExistsAsync("Patient").GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole("Patient")).GetAwaiter().GetResult();
                return Content("Patient role created successfully. Subsequent users will get Patient role.");
            }
            else
            {
                return Content("Patient role already exists.");
            }
        }

        // Temporary action to cleanup all patients and admin users - REMOVE IN PRODUCTION
        public IActionResult CleanupUsers()
        {
            var allUsers = _userManager.Users.ToList();
            var usersToDelete = new List<ApplicationUser>();
            var doctors = context.Doctors.Select(d => d.UserId).ToList();

            foreach (var user in allUsers)
            {
                var roles = _userManager.GetRolesAsync(user).GetAwaiter().GetResult();

                // Delete if user is not a doctor (not in Doctors table)
                if (!doctors.Contains(user.Id))
                {
                    usersToDelete.Add(user);
                }
            }

            int deletedCount = 0;
            foreach (var user in usersToDelete)
            {
                // Delete associated patient record if exists FIRST
                var patient = context.Patients.FirstOrDefault(p => p.UserId == user.Id);
                if (patient != null)
                {
                    // Delete associated appointments FIRST
                    var appointments = context.Appointments.Where(a => a.PatientId == patient.Id).ToList();
                    if (appointments.Any())
                    {
                        context.Appointments.RemoveRange(appointments);
                        context.SaveChangesAsync().GetAwaiter().GetResult();
                    }

                    // Then delete the patient
                    context.Patients.Remove(patient);
                    context.SaveChangesAsync().GetAwaiter().GetResult();
                }

                // Delete user
                var result = _userManager.DeleteAsync(user).GetAwaiter().GetResult();
                if (result.Succeeded)
                {
                    deletedCount++;
                }
            }

            // Delete Patient and Admin roles
            var patientRole = _roleManager.FindByNameAsync("Patient").GetAwaiter().GetResult();
            if (patientRole != null)
            {
                _roleManager.DeleteAsync(patientRole).GetAwaiter().GetResult();
            }

            var adminRole = _roleManager.FindByNameAsync("Admin").GetAwaiter().GetResult();
            if (adminRole != null)
            {
                _roleManager.DeleteAsync(adminRole).GetAwaiter().GetResult();
            }

            return Content($"Deleted {deletedCount} users (patients and admins). Only doctors remain. Admin and Patient roles removed.");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
