// Domain/Entities/Provider.cs
namespace Clinix.Domain.Entities;

using Clinix.Domain.Abstractions;

public sealed class Provider : Entity
    {
    public string Name { get; private set; } = "";
    public string Specialty { get; private set; } = "";
    public string? Tags { get; private set; }
    public DateTime WorkStartTime { get; private set; } = DateTime.Today.AddHours(9);  // 09:00
    public DateTime WorkEndTime { get; private set; } = DateTime.Today.AddHours(17);   // 17:00

    private Provider() { }
    public Provider(string name, string specialty, string? tags, DateTime start, DateTime end)
        { Name = name; Specialty = specialty; Tags = tags; WorkStartTime = start; WorkEndTime = end; }

    public void UpdateWorkingHours(DateTime start, DateTime end)
        {
        WorkStartTime = start;
        WorkEndTime = end;
        }
    }
