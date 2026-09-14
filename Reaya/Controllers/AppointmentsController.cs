using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reaya.Data;
using Reaya.Models;

namespace Reaya.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AppointmentsController : Controller
    {
        AppDbContext context = new AppDbContext();
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentsController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

    public IActionResult Index(string? search, string? sortBy, string? sortOrder)
 {
    // Get current user
    var user = _userManager.GetUserAsync(User).GetAwaiter().GetResult();

    // Sidebar data
    ViewData["SidebarType"] = "admin";
    ViewData["UserName"] = user?.FullName ?? user?.Email ?? "Admin";
    ViewData["UserRole"] = "Front desk admin";

    ViewBag.TotalAppointments = context.Appointments.Count();
    ViewBag.ConfirmedAppointments = context.Appointments.Count(a => a.Status == AppointmentStatus.Confirmed);
    ViewBag.PendingAppointments = context.Appointments.Count(a => a.Status == AppointmentStatus.Pending);
    ViewBag.CompletedAppointments = context.Appointments.Count(a => a.Status == AppointmentStatus.Completed);

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

    var appointments = query.ToList();

    return View(appointments);
}

        public IActionResult Create(){
            // Get current user
            var user = _userManager.GetUserAsync(User).GetAwaiter().GetResult();

            // Sidebar data
            ViewData["SidebarType"] = "admin";
            ViewData["UserName"] = user?.FullName ?? user?.Email ?? "Admin";
            ViewData["UserRole"] = "Front desk admin";

            ViewBag.Specializations = context.Doctors
                .Select(d => d.Specialization)
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct()
                .ToList();

            return View();
        }

        public IActionResult Edit(int id){
            // Get current user
            var user = _userManager.GetUserAsync(User).GetAwaiter().GetResult();

            // Sidebar data
            ViewData["SidebarType"] = "admin";
            ViewData["UserName"] = user?.FullName ?? user?.Email ?? "Admin";
            ViewData["UserRole"] = "Front desk admin";

            var appointment = context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefault(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            ViewBag.Specializations = context.Doctors
                .Select(d => d.Specialization)
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct()
                .ToList();

            // Also load doctors for the appointment's doctor's specialization
            if (appointment.Doctor != null)
            {
                ViewBag.DoctorsInSpecialization = context.Doctors
                    .Where(d => d.Specialization == appointment.Doctor.Specialization)
                    .ToList();
            }

            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CurrentEdit(Appointment appointment){
      

            if (ModelState.IsValid)
            {
                context.Appointments.Update(appointment);
                context.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Specializations = context.Doctors
                .Select(d => d.Specialization)
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct()
                .ToList();

            return View("Edit", appointment);
        }

        public IActionResult Details(int id)
        {
            // Get current user
            var user = _userManager.GetUserAsync(User).GetAwaiter().GetResult();

            ViewData["SidebarType"] = "admin";
            ViewData["UserName"] = user?.FullName ?? user?.Email ?? "Admin";
            ViewData["UserRole"] = "Front desk admin";

            var appointment = context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefault(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        public IActionResult Delete(int id)
        {
            // Sidebar data
            ViewData["SidebarType"] = "admin";
            ViewData["UserName"] = "Rana Kamal";
            ViewData["UserRole"] = "Front desk admin";

            var appointment = context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefault(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CurrentDelete(int id)
        {
            var appointment = context.Appointments.Find(id);
            if (appointment != null)
            {
                context.Appointments.Remove(appointment);
                context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CurrentCreate(Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                context.Appointments.Add(appointment);
                context.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Specializations = context.Doctors
                .Select(d => d.Specialization)
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct()
                .ToList();

            return View("Create", appointment);
        }

        // GET: /Appointments/GetDoctorsBySpecialization?specialization=Cardiology
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

       
        [HttpGet]
        public IActionResult SearchPatients(string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                return Json(new List<object>());

            var patients = context.Patients
                .Where(p => p.Name.Contains(q) || p.PhoneNo.Contains(q))
                .Select(p => new { p.Id, p.Name, p.PhoneNo })
                .Take(8)
                .ToList();

            return Json(patients);
        }
    }
}

