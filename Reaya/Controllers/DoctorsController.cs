using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reaya.Data;
using Reaya.Models;

namespace Reaya.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class DoctorsController : Controller
    
    {
        AppDbContext context = new AppDbContext();

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _webHostEnvironment;

        public DoctorsController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            Microsoft.AspNetCore.Hosting.IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index(string? search , string? specialization)
        {
            // Sidebar data
            ViewData["SidebarType"] = "admin";
            ViewData["UserName"] = "Rana Kamal";
            ViewData["UserRole"] = "Front desk admin";
            
            IQueryable<Doctor> doctor = context.Doctors;

            if (!string.IsNullOrEmpty(search))
            {
                doctor = doctor.Where(d => d.Name.Contains(search) 
                || d.Specialization.Contains(search));
            }

            if (!string.IsNullOrEmpty(specialization))
            {
                doctor = doctor.Where(d => d.Specialization == specialization);
            }

            return View(doctor.ToList());
        }

        public IActionResult Edit(int id)
        {
            // Sidebar data
            ViewData["SidebarType"] = "admin";
            ViewData["UserName"] = "Rana Kamal";
            ViewData["UserRole"] = "Front desk admin";

            var doctor = context.Doctors.Find(id);
            if (doctor == null)
            {
                return NotFound();
            }
            return View(doctor);
        }

        public IActionResult Delete(int id){
            // Sidebar data
            ViewData["SidebarType"] = "admin";
            ViewData["UserName"] = "Rana Kamal";
            ViewData["UserRole"] = "Front desk admin";

            var doctor = context.Doctors.Find(id);
            if (doctor == null)
            {
                return NotFound();
            }
            return View(doctor);
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
        public IActionResult CurrentCreate(Doctor doctor){
            string password = Request.Form["password"].ToString();

            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(string.Empty, "Password is required.");
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = doctor.Email,
                    Email = doctor.Email,
                    FullName = doctor.Name,
                    PhoneNumber = doctor.Phone,
                    EmailConfirmed = true
                };

                var result = _userManager.CreateAsync(user, password).GetAwaiter().GetResult();

                if (result.Succeeded)
                {
                    if (!_roleManager.RoleExistsAsync("Doctor").GetAwaiter().GetResult())
                    {
                        _roleManager.CreateAsync(new IdentityRole("Doctor")).GetAwaiter().GetResult();
                    }

                    _userManager.AddToRoleAsync(user, "Doctor").GetAwaiter().GetResult();

                    doctor.UserId = user.Id;

                    if (doctor.ImageFile == null)
                    {
                        doctor.ImagePath = "\\images\\user_default.jpg";
                    }
                    else
                    {
                        Guid imageGuid = Guid.NewGuid();
                        string imageExtension = System.IO.Path.GetExtension(doctor.ImageFile.FileName);
                        string imageNewName = imageGuid + imageExtension;
                        doctor.ImagePath = "\\images\\" + imageNewName;
                        string imageFullPath = _webHostEnvironment.WebRootPath + doctor.ImagePath;
                        using (var imageFileStream = new System.IO.FileStream(imageFullPath, System.IO.FileMode.Create))
                        {
                            doctor.ImageFile.CopyTo(imageFileStream);
                        }
                    }

                    context.Doctors.Add(doctor);
                    context.SaveChanges();

                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errors)
            {
                Console.WriteLine("Validation Error: " + error.ErrorMessage);
            }

            return View("Create", doctor);
        }
    
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CurrentEdit(Doctor doctor) {
            if (ModelState.IsValid)
            {
                var existingDoctor = context.Doctors.AsNoTracking().FirstOrDefault(d => d.Id == doctor.Id);
                if (existingDoctor == null)
                {
                    return NotFound();
                }

                doctor.UserId = existingDoctor.UserId;

                if (doctor.ImageFile != null)
                {
                    Guid imageGuid = Guid.NewGuid();
                    string imageExtension = System.IO.Path.GetExtension(doctor.ImageFile.FileName);
                    string imageNewName = imageGuid + imageExtension;
                    doctor.ImagePath = "\\images\\" + imageNewName;
                    string imageFullPath = _webHostEnvironment.WebRootPath + doctor.ImagePath;
                    using (var imageFileStream = new System.IO.FileStream(imageFullPath, System.IO.FileMode.Create))
                    {
                        doctor.ImageFile.CopyTo(imageFileStream);
                    }

                    // Delete old image if it's not the default one
                    if (!string.IsNullOrEmpty(existingDoctor.ImagePath) && existingDoctor.ImagePath != "\\images\\user_default.jpg")
                    {
                        string oldImagePath = _webHostEnvironment.WebRootPath + existingDoctor.ImagePath;
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                }
                else
                {
                    // keep old image
                    doctor.ImagePath = existingDoctor.ImagePath;
                }

                context.Doctors.Update(doctor);
                context.SaveChanges();
                
                return RedirectToAction("Index");
            }

            return View("Edit", doctor);
}
    

        public IActionResult CurrentDelete(int id) {
            var doctor = context.Doctors.Find(id);
            if (doctor == null)
            {
                return NotFound();
            }

            string? userId = doctor.UserId;

            var appointments = context.Appointments.Where(a => a.DoctorId == id).ToList();
            if (appointments.Any())
            {
                context.Appointments.RemoveRange(appointments);
            }

            if (!string.IsNullOrEmpty(doctor.ImagePath) && doctor.ImagePath != "\\images\\user_default.jpg")
            {
                string oldImagePath = _webHostEnvironment.WebRootPath + doctor.ImagePath;
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            context.Doctors.Remove(doctor);
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

            var doctor = context.Doctors.Find(id);
            if (doctor == null)
            {
                return NotFound();
            }
            return View(doctor);
        }
    }

}
