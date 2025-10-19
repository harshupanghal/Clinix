//using System.ComponentModel.DataAnnotations;

//namespace Clinix.Domain.Entities.FollowUp;

///// <summary>
///// Prescription entity linked to Appointment. Supports 1-to-many meds via nested collection.
///// </summary>
//public class Prescription
//    {
//    [Key]
//    public long Id { get; set; }

//    public long AppointmentId { get; set; }
//    public long PatientId { get; set; }
//    public long DoctorId { get; set; }

//    [MaxLength(512)]
//    public string? Diagnosis { get; set; }

//    [MaxLength(2000)]
//    public string? DoctorNotes { get; set; }

//    // Option: store medications normalized or as JSON. We'll include a normalized child table for clarity.
//    public ICollection<PrescriptionMedication>? Medications { get; set; }

//    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
//    public long CreatedByUserId { get; set; }
//    }

//public class PrescriptionMedication
//    {
//    [Key]
//    public long Id { get; set; }
//    public long PrescriptionId { get; set; }
//    [MaxLength(256)]
//    public string Name { get; set; } = string.Empty;
//    [MaxLength(256)]
//    public string? Dosage { get; set; }
//    [MaxLength(256)]
//    public string? Frequency { get; set; }
//    }
