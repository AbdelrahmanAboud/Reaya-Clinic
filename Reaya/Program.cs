using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reaya.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("AppDbContext") ?? throw new InvalidOperationException("Connection string 'AppDbContext' not found.");

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, EmailJsEmailSender>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer("Server=.\\SQLEXPRESS;Database=Reaya;Trusted_Connection=True;TrustServerCertificate=True;"));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => 
options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("DoctorOnly", policy => policy.RequireRole("Doctor"));
    options.AddPolicy("PatientOnly", policy => policy.RequireRole("Patient"));
    options.AddPolicy("AdminOrDoctor", policy => policy.RequireRole("Admin", "Doctor"));
    options.AddPolicy("AdminOrPatient", policy => policy.RequireRole("Admin", "Patient"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); 

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();