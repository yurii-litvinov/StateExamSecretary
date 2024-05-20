namespace ScheduleParser.Models;

public class StudentWork(int number, string studentName, string theme, string supervisor, string consultant, string reviewer)
{
    public int Number = number;
    public string StudentName = studentName;
    public string Theme = theme;
    public string Supervisor = supervisor;
    public string Consultant = consultant;
    public string Reviewer = reviewer;
}
