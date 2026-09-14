using System;
using System.Collections.Generic;

namespace Reaya.TempDb;

public partial class MedicalRecord
{
    public int Id { get; set; }

    public int AppointmentId { get; set; }

    public string Diagnosis { get; set; } = null!;

    public string Prescription { get; set; } = null!;

    public string DoctorNotes { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;
}
