//using Clinix.Domain.Entities.ApplicationUsers;
//using Clinix.Domain.Entities.Appointments;
//using Clinix.Infrastructure.Persistence;
//using Microsoft.EntityFrameworkCore;

//namespace Clinix.Infrastructure.Data;

//public static class DataSeeder2
//    {
//    public static async Task SeedAsync(ClinixDbContext db)
//        {
//        if (await db.Doctors.AnyAsync()) return; // already seeded

//        var doctors = new List<Doctor>
//        {
//            new Doctor(, "Dr. Alice Smith", "Cardiology"),
//            new Doctor(, "Dr. Bob Jones", "Dermatology"),
//            new Doctor(, "Dr. Carol Lee", "Neurology")
//        };

//        var patients = new List<Patient>
//        {
//            new Patient( "John Doe", "john@example.com", "+911234567890"),
//            new Patient(, "Jane Roe", "jane@example.com", "+919876543210")
//        };

//        var workingHours = new List<DoctorWorkingHours>();

//        foreach (var doc in doctors)
//            {
//            workingHours.Add(new DoctorWorkingHours
//                {
//                Id = doc.Id,
//                WeeklyHours = new Dictionary<DayOfWeek, List<(TimeSpan Start, TimeSpan End)>>
//                {
//                    { DayOfWeek.Monday, new List<(TimeSpan, TimeSpan)>{ (TimeSpan.FromHours(9), TimeSpan.FromHours(17)) } },
//                    { DayOfWeek.Tuesday, new List<(TimeSpan, TimeSpan)>{ (TimeSpan.FromHours(9), TimeSpan.FromHours(17)) } },
//                    { DayOfWeek.Wednesday, new List<(TimeSpan, TimeSpan)>{ (TimeSpan.FromHours(9), TimeSpan.FromHours(17)) } },
//                    { DayOfWeek.Thursday, new List<(TimeSpan, TimeSpan)>{ (TimeSpan.FromHours(9), TimeSpan.FromHours(17)) } },
//                    { DayOfWeek.Friday, new List<(TimeSpan, TimeSpan)>{ (TimeSpan.FromHours(9), TimeSpan.FromHours(17)) } }
//                }
//                });
//            }

//        await db.Doctors.AddRangeAsync(doctors);
//        await db.Patients.AddRangeAsync(patients);
//        await db.DoctorWorkingHours.AddRangeAsync(workingHours);

//        await db.SaveChangesAsync();
//        }
//    }

