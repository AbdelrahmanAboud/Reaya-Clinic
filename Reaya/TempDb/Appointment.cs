using System;
using System.Collections.Generic;

namespace Reaya.TempDb;

public partial class Appointment
{
    public int Id { get; set; }

    public string? Notes { get; set; }

    public DateTime AppointmentDate { get; set; }

    public int Status { get; set; }

    public int? PatientId { get; set; }

    public int DoctorId { get; set; }

    public string? GuestPatientName { get; set; }

    public string? GuestPatientPhone { get; set; }

    public virtual Doctor Doctor { get; set; } = null!;

    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();

    public virtual Patient? Patient { get; set; }
}
