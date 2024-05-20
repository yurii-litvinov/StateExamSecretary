namespace ScheduleParser.Models;

public class DaySchedule(string date, List<string> commissionMembers, List<CommissionMeeting> commissionMeetings)
{
    public readonly string Date = date;
    public List<string> CommissionMembers = commissionMembers;
    public readonly List<CommissionMeeting> CommissionMeetings = commissionMeetings;
}