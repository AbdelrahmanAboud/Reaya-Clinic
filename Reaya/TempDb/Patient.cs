using System;
using System.Collections.Generic;

namespace Reaya.TempDb;

public partial class Patient
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PhoneNo { get; set; } = null!;

    public string Gender { get; set; } = null!;

    public DateTime DateOfBirth { get; set; }

    public string Address { get; set; } = null!;

    public string BloodType { get; set; } = null!;

    public string? UserId { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual AspNetUser? User { get; set; }
}
