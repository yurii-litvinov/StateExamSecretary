namespace ScheduleParser.Models;

public class CommissionMeeting(string timeAndAuditorium, string meetingInfo, List<StudentWork> studentWorks)
{
    public string TimeAndAuditorium = timeAndAuditorium;
    public string MeetingInfo = meetingInfo;
    public List<StudentWork> StudentWorks = studentWorks;
}