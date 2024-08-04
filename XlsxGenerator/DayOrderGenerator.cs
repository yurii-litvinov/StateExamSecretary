// <copyright file="DayOrderGenerator.cs" company="Maria Myasnikova">
// Copyright (c) Maria Myasnikova. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace XlsxGenerator;

using NPOI.SS.UserModel;
using ScheduleParser.Models;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Class to generate the orders of the day.
/// </summary>
[SuppressMessage(
    "StyleCop.CSharp.SpacingRules",
    "SA1000:Keywords should be spaced correctly",
    Justification = "Causes another problem with spaces.")]
public class DayOrderGenerator
{
    private int index;

    /// <summary>
    /// Generates the order of the day for the SES.
    /// </summary>
    /// <returns>.xlsx file as a stream.</returns>
    public Stream GenerateSES(DaySchedule day)
    {
        var generator = new XlsxGenerator();
        var sheet = generator.CreateSheet();

        var header = new List<CellInfo>
        {
            new("ФИО"), new("Оценки", 2),
            new("Тема"), new("Научрук"), new("Консультант"), new("Рецензент"),
        };

        var links = new List<CellInfo>
        {
            new("Созвон для защиты:"),
            new("Созвон для закрытого обсуждения ГЭК:"),
            new("Материалы:"),
        };

        var cursiveRow = new List<CellInfo>
        {
            new("Член комиссии", 2),
            new("Оценка", 2),
            new("Комментарий", 4),
        };

        var members = day.CommissionMembers.Select(member => new CellInfo(member, 2)).ToList();

        sheet.WriteRow(header, true, false, HorizontalAlignment.Center, columnIndex: 1);

        foreach (var meeting in day.CommissionMeetings)
        {
            var subheader = new List<CellInfo>
            {
                new(meeting.TimeAndAuditorium), new("Рук"), new("Рец"), new(meeting.MeetingInfo),
            };

            sheet.WriteRow(subheader, true, false, HorizontalAlignment.Center, columnIndex: 1);
            sheet.WriteColumn(links, true, false, HorizontalAlignment.Right, columnIndex: 1);

            this.index = sheet.GetLastRowIndex();
            for (var row = this.index - links.Count + 1; row <= this.index; row++)
            {
                sheet.MergeCells(row, row, 2, header.Count + 1);
            }

            foreach (var work in meeting.StudentWorks)
            {
                var studentWork = new List<CellInfo>
                {
                    new(work.Number.ToString()),
                    new(work.StudentName),
                    new(string.Empty),
                    new(string.Empty),
                    new(work.Theme),
                    new(work.Supervisor),
                    new(work.Consultant ?? string.Empty),
                    new(work.Reviewer),
                };

                sheet.WriteRow(studentWork, true);
                sheet.WriteRow(cursiveRow, false, true, HorizontalAlignment.Center);
                sheet.WriteColumn(members);

                // Merging cells for marks and comments of commission members.
                this.index = sheet.GetLastRowIndex();
                for (var row = this.index - members.Count + 1; row <= this.index; row++)
                {
                    sheet.MergeCells(row, row, 2, 3);
                    sheet.MergeCells(row, row, 4, header.Count + 1);
                }

                sheet.WriteEmptyRows(1);

                this.index = sheet.GetLastRowIndex();
                sheet.MergeCells(this.index, this.index, 0, header.Count + 1);
            }

            sheet.WriteEmptyRows(2);

            this.index = sheet.GetLastRowIndex();
            for (var i = 0; i < 2; i++)
            {
                sheet.MergeCells(this.index - i, this.index - i, 0, header.Count + 1);
            }
        }

        sheet.AutosizeColumns(0, header.Count + 1);
        sheet.SetColumnWidth(0, 3);
        sheet.SetColumnWidth(2, 10);
        sheet.SetColumnWidth(3, 10);
        sheet.SetColumnWidth(4, 65);
        sheet.SetColumnWidth(5, 30);
        sheet.SetColumnWidth(6, 37);
        sheet.SetColumnWidth(7, 30);

        sheet.SetDefaultRowHeight(35);

        return generator.GetStream();
    }

    /// <summary>
    /// Generates the order of the day for the public.
    /// </summary>
    /// <returns>.xlsx file as a stream.</returns>
    public Stream GeneratePublic(DaySchedule day)
    {
        var generator = new XlsxGenerator();
        var sheet = generator.CreateSheet();

        var header = new List<CellInfo>
        {
            new("ФИО"), new("Тема"), new("Научрук"), new("Консультант"), new("Рецензент"),
        };

        var links = new List<CellInfo>
        {
            new("Созвон для защиты:"),
            new("Материалы:"),
        };

        sheet.WriteRow(header, true, false, HorizontalAlignment.Center, columnIndex: 1);

        foreach (var meeting in day.CommissionMeetings)
        {
            var subheader = new List<CellInfo>
            {
                new(meeting.TimeAndAuditorium), new(meeting.MeetingInfo),
            };

            sheet.WriteRow(subheader, true, false, HorizontalAlignment.Center, columnIndex: 1);
            sheet.WriteColumn(links, true, false, HorizontalAlignment.Right, columnIndex: 1);

            this.index = sheet.GetLastRowIndex();
            for (var row = this.index - links.Count + 1; row <= this.index; row++)
            {
                sheet.MergeCells(row, row, 2, header.Count);
            }

            foreach (var work in meeting.StudentWorks)
            {
                var studentWork = new List<CellInfo>
                {
                    new(work.Number.ToString()),
                    new(work.StudentName),
                    new(work.Theme),
                    new(work.Supervisor),
                    new(work.Consultant ?? string.Empty),
                    new(work.Reviewer),
                };

                sheet.WriteRow(studentWork);
            }

            sheet.WriteEmptyRows(1);
        }

        sheet.AutosizeColumns(0, header.Count);

        sheet.SetColumnWidth(0, 3);
        sheet.SetColumnWidth(2, 65);
        sheet.SetColumnWidth(3, 30);
        sheet.SetColumnWidth(4, 37);
        sheet.SetColumnWidth(5, 30);

        sheet.SetDefaultRowHeight(35);

        return generator.GetStream();
    }
}