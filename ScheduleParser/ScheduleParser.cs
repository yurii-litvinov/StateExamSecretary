using System.Text.RegularExpressions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using ScheduleParser.Models;

namespace ScheduleParser;

/// <summary>
/// Class for parsing the schedule.
/// </summary>
/// <param name="config">Instance of the <see cref="Config"/> class containing
/// paths or links to files with schedule and work themes.</param>
public class ScheduleParser(Config.Config config)
{
    private int _rowIndex = 1;
    private readonly List<DaySchedule> _days = [];
    private const string TextInBracketsPattern = @" \([^)]*\)";

    /// <summary>
    /// Parse the schedule for the days.
    /// </summary>
    /// <returns>List of days <see cref="DaySchedule"/>.</returns>
    public List<DaySchedule> Parse()
    {
        var scheduleWorkbook = new XSSFWorkbook(GetStream(config.Schedule));
        scheduleWorkbook.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
        var sheet = scheduleWorkbook.GetSheetAt(0);

        while (_rowIndex < sheet.LastRowNum)
        {
            FetchDay(sheet);
            _rowIndex++;
        }

        return _days;
    }

    /// <summary>
    /// Adds a new day to the list of days or updates information about the last day.
    /// </summary>
    /// <param name="sheet">Sheet with schedule.</param>
    private void FetchDay(ISheet sheet)
    {
        var row = sheet.GetRow(_rowIndex);
        var cells = new List<List<ICell>>();

        while (!CellsAreEmpty(row.Cells))
        {
            if (!CellsAreEmpty(row.Cells[1..]))
            {
                var rowCells = new List<ICell>();
                for (var i = 0; i < 8; i++)
                {
                    rowCells.Add(row.GetCell(i));
                }

                cells.Add(rowCells);
            }

            _rowIndex++;
            row = sheet.GetRow(_rowIndex);
        }

        var date = Regex.Replace(cells[0][1].StringCellValue, TextInBracketsPattern, "");
        var meeting = FetchMeeting(cells[1..]);

        if (!IsMeetingCorrect(meeting.MeetingInfo))
        {
            return;
        }

        FetchConsultants(meeting);

        if (_days.Count != 0 && _days.Last().Date == date)
        {
            _days.Last().CommissionMeetings.Add(meeting);
        }
        else
        {
            var members = cells
                .TakeWhile(rowCells => rowCells[6].CellType != CellType.Blank)
                .Select(rowCells => rowCells[6].StringCellValue)
                .ToList();

            var day = new DaySchedule(date, members, [meeting]);
            _days.Add(day);
        }
    }
    
    /// <summary>
    /// Fetches information about the commission meeting.
    /// </summary>
    /// <param name="cells">Cells with information about the meeting.</param>
    /// <returns>Instance of <see cref="CommissionMeeting"/> class.</returns>
    private CommissionMeeting FetchMeeting(List<List<ICell>> cells)
    {
        var timeAndAuditorium = cells[0][1].StringCellValue;
        var meetingInfo = cells[0][2].StringCellValue;
        var studentWorks = cells[1..].Select(FetchStudentWork)
            .Where(work => work.StudentName != "").ToList();

        return new CommissionMeeting(timeAndAuditorium, meetingInfo, studentWorks);
    }
    
    /// <summary>
    /// Adds consultants from tables with themes for students at the meeting.
    /// </summary>
    private void FetchConsultants(CommissionMeeting meeting)
    {
        var infoSplit = meeting.MeetingInfo.Split(", ");
        var chairSheet = GetChairSheet(infoSplit[0], infoSplit[1]);

        var studentsAndConsultants = new List<(string, string)>();
        for (var i = 1; i < chairSheet.LastRowNum; i++)
        {
            var row = chairSheet.GetRow(i);
            studentsAndConsultants.Add(
                (row.GetCell(0).StringCellValue, row.GetCell(4).StringCellValue)
            );
        }

        foreach (var studentWork in meeting.StudentWorks)
        {
            var student = studentWork.StudentName;
            foreach (var pair in studentsAndConsultants.Where(pair => student == pair.Item1))
            {
                studentWork.Consultant = pair.Item2;
            }
        }
    }
    
    /// <summary>
    /// Gets the chair sheet from the table with themes related to this level of education.
    /// </summary>
    private ISheet GetChairSheet(string chair, string level)
    {
        var stream = GetStream(level switch
        {
            "бакалавры МОиАИС" => config.BachelorsMs,
            "бакалавры ПИ" => config.BachelorsSe,
            "магистры МОиАИС" => config.MastersMs,
            "магистры ПИ" => config.MastersSe,
            _ => throw new ArgumentException($"Неверный уровень образования: {level}")
        });
        var workbook = new XSSFWorkbook(stream);

        return chair == "Информатика/ПА"
            ? MergeSheets([workbook.GetSheet("Информатики"), workbook.GetSheet("ПА")])
            : workbook.GetSheet(chair);
    }
    
    /// <summary>
    /// Merge sheets with same header.
    /// </summary>
    private ISheet MergeSheets(List<ISheet> sheets)
    {
        var newSheet = new XSSFWorkbook().CreateSheet();
        var newRowIndex = 0;

        var header = newSheet.CreateRow(newRowIndex);
        foreach (var cell in sheets[0].GetRow(0))
        {
            header.CreateCell(cell.ColumnIndex).SetCellValue(cell.StringCellValue);
        }

        foreach (var sheet in sheets)
        {
            for (var i = 1; i < sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);
                newRowIndex++;
                var newRow = newSheet.CreateRow(newRowIndex);
                foreach (var cell in row)
                {
                    newRow.CreateCell(cell.ColumnIndex).SetCellValue(cell.StringCellValue);
                }
            }
        }

        return newSheet;
    }
    
    /// <summary>
    /// Fetches information about the student work.
    /// </summary>
    /// <param name="cells">Cells with information about the student work.</param>
    /// <returns>Instance of <see cref="StudentWork"/> class.</returns>
    private StudentWork FetchStudentWork(List<ICell> cells)
    {
        var number = cells[0].NumericCellValue;
        var studentName = cells[1].StringCellValue;
        var theme = cells[2].StringCellValue;
        var supervisor = cells[3].StringCellValue;
        var reviewer = cells[4].StringCellValue;

        return new StudentWork((int)number, studentName, theme, supervisor, "", reviewer);
    }

    private bool CellsAreEmpty(List<ICell> cells)
    {
        return cells.All(cell => cell.CellType == CellType.Blank);
    }
    
    /// <summary>
    /// Verifies that the meeting is dedicated to the work of bachelors or masters.
    /// </summary>
    /// <param name="info">Information about the meeting.</param>
    private bool IsMeetingCorrect(string info)
    {
        return info.Contains("бакалавры", StringComparison.OrdinalIgnoreCase) ||
               info.Contains("магистры", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the string is a path or a link to the file and returns the stream.
    /// </summary>
    private Stream GetStream(string path)
    {
        return Uri.IsWellFormedUriString(path, UriKind.Absolute)
            ? YandexDiskDownloader.DownloadFile(path).Result
            : new FileStream(path, FileMode.Open, FileAccess.Read);
    }
}