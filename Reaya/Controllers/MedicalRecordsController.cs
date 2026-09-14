using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reaya.Data;
using Reaya.Models;

namespace Reaya.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class MedicalRecordsController : Controller
    {
        AppDbContext context = new AppDbContext();

        public IActionResult Index(string? search)
        {
            // Sidebar data
            ViewData["SidebarType"] = "admin";
            ViewData["UserName"] = "Rana Kamal";
            ViewData["UserRole"] = "Front desk admin";

            IQueryable<MedicalRecord> medicalRecords = context.MedicalRecords
                .Include(mr => mr.Appointment)
                .ThenInclude(a => a.Patient)
                .Include(mr => mr.Appointment)
                .ThenInclude(a => a.Doctor);

            if (!string.IsNullOrEmpty(search))
            {
                medicalRecords = medicalRecords.Where(mr => 
                    mr.Diagnosis.Contains(search) || 
                    mr.Prescription.Contains(search) ||
                    mr.Appointment.Patient.Name.Contains(search) ||
                    mr.Appointment.Doctor.Name.Contains(search));
            }

            return View(medicalRecords.OrderByDescending(mr => mr.CreatedAt).ToList());
        }

        public IActionResult Create(int appointmentId)
        {
            // Sidebar data
            ViewData["SidebarType"] = "admin";
            ViewData["UserName"] = "Rana Kamal";
            ViewData["UserRole"] = "Front desk admin";

            var appointment = context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefault(a => a.Id == appointmentId);

            if (appointment == null)
            {
                return NotFound();
            }

            ViewBag.Appointment = appointment;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CurrentCreate(MedicalRecord medicalRecord, int appointmentId)
        {
            if (ModelState.IsValid)
            {
                medicalRecord.AppointmentId = appointmentId;
                medicalRecord.CreatedAt = DateTime.Now;
                
                context.MedicalRecords.Add(medicalRecord);
                context.SaveChanges();

                // Update appointment status to completed
                var appointmentToUpdate = context.Appointments.Find(appointmentId);
                if (appointmentToUpdate != null)
                {
                    appointmentToUpdate.Status = AppointmentStatus.Completed;
                    context.SaveChanges();
                }

                return RedirectToAction("Index");
            }

            var appointment = context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefault(a => a.Id == appointmentId);
            ViewBag.Appointment = appointment;

            return View("Create", medicalRecord);
        }

        public IActionResult Edit(int id)
        {
            // Sidebar data
            ViewData["SidebarType"] = "admin";
            ViewData["UserName"] = "Rana Kamal";
            ViewData["UserRole"] = "Front desk admin";

            var medicalRecord = context.MedicalRecords
                .Include(mr => mr.Appointment)
                .ThenInclude(a => a.Patient)
                .Include(mr => mr.Appointment)
                .ThenInclude(a => a.Doctor)
                .FirstOrDefault(mr => mr.Id == id);

            if (medicalRecord == null)
            {
                return NotFound();
            }

            return View(medicalRecord);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CurrentEdit(MedicalRecord medicalRecord)
        {
            if (ModelState.IsValid)
            {
                var existingRecord = context.MedicalRecords.AsNoTracking().FirstOrDefault(mr => mr.Id == medicalRecord.Id);
                if (existingRecord == null)
                {
                    return NotFound();
                }

                medicalRecord.AppointmentId = existingRecord.AppointmentId;
                medicalRecord.CreatedAt = existingRecord.CreatedAt;

                context.MedicalRecords.Update(medicalRecord);
                context.SaveChanges();

                return RedirectToAction("Index");
            }

            return View("Edit", medicalRecord);
        }

        public IActionResult Delete(int id)
        {
            // Sidebar data
            ViewData["SidebarType"] = "admin";
            ViewData["UserName"] = "Rana Kamal";
            ViewData["UserRole"] = "Front desk admin";

            var medicalRecord = context.MedicalRecords
                .Include(mr => mr.Appointment)
                .ThenInclude(a => a.Patient)
                .Include(mr => mr.Appointment)
                .ThenInclude(a => a.Doctor)
                .FirstOrDefault(mr => mr.Id == id);

            if (medicalRecord == null)
            {
                return NotFound();
            }

            return View(medicalRecord);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CurrentDelete(int id)
        {
            var medicalRecord = context.MedicalRecords.Find(id);
            if (medicalRecord == null)
            {
                return NotFound();
            }

            context.MedicalRecords.Remove(medicalRecord);
            context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Details(int id)
        {
            // Sidebar data
            ViewData["SidebarType"] = "admin";
            ViewData["UserName"] = "Rana Kamal";
            ViewData["UserRole"] = "Front desk admin";

            var medicalRecord = context.MedicalRecords
                .Include(mr => mr.Appointment)
                .ThenInclude(a => a.Patient)
                .Include(mr => mr.Appointment)
                .ThenInclude(a => a.Doctor)
                .FirstOrDefault(mr => mr.Id == id);

            if (medicalRecord == null)
            {
                return NotFound();
            }

            return View(medicalRecord);
        }
    }
}