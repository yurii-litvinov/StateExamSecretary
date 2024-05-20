using System.Text.Json;
using ScheduleParser.Config;

var jsonString = File.ReadAllText("../../../test.json");
var config = JsonSerializer.Deserialize<Config>(jsonString);
try
{
    var parser = new ScheduleParser.ScheduleParser(config!);
    var days = parser.Parse();
    foreach (var day in days)
    {
        Console.WriteLine(day.Date);
        foreach (var meeting in day.CommissionMeetings)
        {
            Console.WriteLine($"{meeting.TimeAndAuditorium}, {meeting.MeetingInfo}");
            foreach (var studentWork in meeting.StudentWorks)
            {
                Console.WriteLine($"Студент: {studentWork.StudentName}. Консультант: {studentWork.Consultant}");
            }
        }
    }
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}