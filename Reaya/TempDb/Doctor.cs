using System;
using System.Collections.Generic;

namespace Reaya.TempDb;

public partial class Doctor
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Specialization { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public decimal ConsultationFee { get; set; }

    public string? UserId { get; set; }

    public string Biography { get; set; } = null!;

    public string YearsExperience { get; set; } = null!;

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual AspNetUser? User { get; set; }
}
