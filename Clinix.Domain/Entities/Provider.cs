namespace Clinix.Domain.Entities;

public class Provider
    {
    public long Id { get; private set; }
    public string Name { get; private set; }
    public string Specialty { get; private set; }
    public string? Tags { get; private set; }
    public DateTime WorkStartTime { get; private set; }
    public DateTime WorkEndTime { get; private set; }

    private Provider() { }

    public Provider(string name, string specialty, string tags, DateTime workStart, DateTime workEnd)
        {
        Name = name;
        Specialty = specialty;
        Tags = tags;
        WorkStartTime = workStart;
        WorkEndTime = workEnd;
        }

    public void UpdateTags(string tags)
        {
        Tags = tags;
        }

    public void UpdateWorkingHours(DateTime start, DateTime end)
        {
        WorkStartTime = start;
        WorkEndTime = end;
        }
    }
