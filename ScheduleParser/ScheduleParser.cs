// <copyright file="ScheduleParser.cs" company="Maria Myasnikova">
// Copyright (c) Maria Myasnikova. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace ScheduleParser;

using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Models;

/// <summary>
/// Class for parsing the schedule.
/// </summary>
/// <param name="config">Instance of the <see cref="Config"/> class containing
/// paths or links to files with schedule and work themes.</param>
[SuppressMessage(
    "StyleCop.CSharp.SpacingRules",
    "SA1010:OpeningSquareBracketsMustBeSpacedCorrectly",
    Justification = "Causes another problem with spaces.")]
public class ScheduleParser(Config.Config config)
{
    private const string TextInBracketsPattern = @" \([^)]*\)";
    private readonly List<DaySchedule> days = [];
    private int rowIndex = 1;

    /// <summary>
    /// Parse the schedule for the days.
    /// </summary>
    /// <returns>List of days <see cref="DaySchedule"/>.</returns>
    public List<DaySchedule> Parse()
    {
        var scheduleWorkbook = new XSSFWorkbook(GetStream(config.Schedule));
        scheduleWorkbook.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
        var sheet = scheduleWorkbook.GetSheetAt(0);

        while (this.rowIndex < sheet.LastRowNum)
        {
            if (!CellsAreEmpty(sheet.GetRow(this.rowIndex).Cells))
            {
                this.FetchDay(sheet);
            }

            this.rowIndex++;
        }

        return this.days;
    }

    /// <summary>
    /// Determines whether the string is a path or a link to the file and returns the stream.
    /// </summary>
    private static Stream GetStream(string path)
        => Uri.IsWellFormedUriString(path, UriKind.Absolute)
            ? YandexDiskDownloader.DownloadFile(path).Result
            : new FileStream(path, FileMode.Open, FileAccess.Read);

    private static bool CellsAreEmpty(List<ICell> cells)
        => cells.All(cell => cell.CellType == CellType.Blank);

    /// <summary>
    /// Verifies that the meeting is dedicated to the work of bachelors or masters.
    /// </summary>
    /// <param name="info">Information about the meeting.</param>
    private static bool IsMeetingCorrect(string info)
        => info.Contains("бакалавры", StringComparison.OrdinalIgnoreCase) ||
           info.Contains("магистры", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Fetches information about the commission meeting.
    /// </summary>
    /// <param name="cells">Cells with information about the meeting.</param>
    /// <returns>Instance of <see cref="CommissionMeeting"/> class.</returns>
    private static CommissionMeeting FetchMeeting(List<List<ICell>> cells)
    {
        var timeAndAuditorium = cells[0][ScheduleColumns.DateTimeAuditorium].StringCellValue.Trim();
        var meetingInfo = cells[0][ScheduleColumns.MeetingInfo].StringCellValue.Trim();
        var studentWorks = cells[1..].Select(FetchStudentWork)
            .Where(work => work.StudentName != string.Empty).ToList();

        return new CommissionMeeting(timeAndAuditorium, meetingInfo, studentWorks);
    }

    /// <summary>
    /// Merge sheets with same header.
    /// </summary>
    private static ISheet MergeSheets(List<ISheet> sheets)
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
    private static StudentWork FetchStudentWork(List<ICell> cells)
    {
        var number = cells[ScheduleColumns.Number].NumericCellValue;
        var studentName = cells[ScheduleColumns.StudentName].StringCellValue.Trim();
        var theme = cells[ScheduleColumns.Theme].StringCellValue.Trim();
        var supervisor = cells[ScheduleColumns.Supervisor].StringCellValue.Trim();
        var reviewer = cells[ScheduleColumns.Reviewer].StringCellValue.Trim();

        return new StudentWork((int)number, studentName, theme, supervisor, reviewer);
    }

    /// <summary>
    /// Adds a new day to the list of days or updates information about the last day.
    /// </summary>
    /// <param name="sheet">Sheet with schedule.</param>
    private void FetchDay(ISheet sheet)
    {
        var row = sheet.GetRow(this.rowIndex);
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

            this.rowIndex++;
            row = sheet.GetRow(this.rowIndex);
        }

        var date = Regex.Replace(
            cells[0][ScheduleColumns.DateTimeAuditorium].StringCellValue,
            TextInBracketsPattern,
            string.Empty);
        var meeting = FetchMeeting(cells[1..]);

        if (!IsMeetingCorrect(meeting.MeetingInfo))
        {
            return;
        }

        this.FetchConsultants(meeting);

        if (this.days.Count != 0 && this.days.Last().Date == date)
        {
            this.days.Last().CommissionMeetings.Add(meeting);
        }
        else
        {
            var members = cells
                .TakeWhile(rowCells => rowCells[ScheduleColumns.CommissionMember].CellType != CellType.Blank)
                .Select(rowCells => rowCells[ScheduleColumns.CommissionMember].StringCellValue.Trim())
                .ToList();

            var day = new DaySchedule(date, members, [meeting]);
            this.days.Add(day);
        }
    }

    /// <summary>
    /// Adds consultants from tables with themes for students at the meeting.
    /// </summary>
    private void FetchConsultants(CommissionMeeting meeting)
    {
        var infoSplit = meeting.MeetingInfo.Split(", ");
        var chairSheet = this.GetChairSheet(infoSplit[0], infoSplit[1]);

        var studentsAndConsultants = new List<(string, string)>();
        for (var i = 1; i < chairSheet.LastRowNum; i++)
        {
            var row = chairSheet.GetRow(i);
            studentsAndConsultants.Add(
                (row.GetCell(ThemesColumns.StudentName).StringCellValue.Trim(),
                    row.GetCell(ThemesColumns.Consultant).StringCellValue.Trim()));
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
        var stream = GetStream(
            level switch
            {
                "бакалавры техпрога" => config.BachelorsPt,
                "бакалавры ПИ" => config.BachelorsSe,
                "магистры техпрога" => config.MastersPt,
                "магистры ПИ" => config.MastersSe,
                _ => throw new ArgumentException($"Неверный уровень образования: {level}")
            });
        var workbook = new XSSFWorkbook(stream);

        return chair == "Информатика/ПА"
            ? MergeSheets([workbook.GetSheet("Информатики"), workbook.GetSheet("ПА")])
            : workbook.GetSheet(chair);
    }

    private static class ScheduleColumns
    {
        public const int Number = 0;
        public const int StudentName = 1;
        public const int DateTimeAuditorium = 1;
        public const int Theme = 2;
        public const int MeetingInfo = 2;
        public const int Supervisor = 3;
        public const int Reviewer = 4;
        public const int CommissionMember = 6;
    }

    private static class ThemesColumns
    {
        public const int StudentName = 0;
        public const int Consultant = 4;
    }
}