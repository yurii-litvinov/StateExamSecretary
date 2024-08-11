// <copyright file="StudentFileVerifier.cs" company="Maria Myasnikova">
// Copyright (c) Maria Myasnikova. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace StateExamSecretaryEngine;

using YandexDisk.Client.Protocol;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using ScheduleParser.Models;

/// <summary>
/// Class for verifying student work files.
/// </summary>
/// <param name="day">Instance of the <see cref="DaySchedule"/> class.</param>
[SuppressMessage(
    "StyleCop.CSharp.SpacingRules",
    "SA1010:Opening square brackets should be spaced correctly",
    Justification = "Causes another problem with spaces.")]
public class StudentFileVerifier(DaySchedule day)
{
    private readonly List<string> correctFilePaths = [];

    /// <summary>
    /// Verifies files in the provided folder.
    /// </summary>
    /// <param name="path">Folder path.</param>
    /// <returns>Paths to the correct files.</returns>
    public List<string> VerifyFiles(string path)
    {
        var filePaths = Directory.GetFiles(path).ToList();

        List<StudentWork> studentWorks = [];
        foreach (var meeting in day.CommissionMeetings)
        {
            studentWorks.AddRange(meeting.StudentWorks);
        }

        foreach (var work in studentWorks)
        {
            var studentFilePaths = FetchStudentFiles(work, filePaths);

            foreach (var filePath in studentFilePaths
                         .Where(filePath => VerifyStudentFile(work, filePath)))
            {
                this.correctFilePaths.Add(filePath);
            }

            if (work.Consultant == null)
            {
                work.HasConsultantReview = true;
            }
        }

        return this.correctFilePaths;
    }

    /// <summary>
    /// Finds student works with missing files.
    /// </summary>
    /// <param name="folderData">Information about the folder on Yandex.Disk.</param>
    /// <returns>Student works with missing files.</returns>
    public IEnumerable<StudentWork> FindWorksWithMissingFiles(Resource folderData)
    {
        List<StudentWork> studentWorks = [];
        foreach (var meeting in day.CommissionMeetings)
        {
            studentWorks.AddRange(meeting.StudentWorks);
        }

        var diskFilePaths = folderData.Embedded.Items.Select(resource => resource.Path).ToList();

        foreach (var work in studentWorks)
        {
            var studentFilePaths = FetchStudentFiles(work, diskFilePaths);

            foreach (var filePath in studentFilePaths)
            {
                VerifyStudentFile(work, filePath);
            }
        }

        return studentWorks.Where(
            work => work is not
            {
                HasReport: true,
                HasPresentation: true,
                HasSupervisorReview: true,
                HasConsultantReview: true,
                HasReviewerReview: true
            }).ToList();
    }

    private static IEnumerable<string> FetchStudentFiles(StudentWork work, List<string> filePaths)
    {
        var nameSplit = work.StudentName.Split(" ");
        var surname = nameSplit[0];
        var name = nameSplit[1];

        return filePaths
            .Where(
                filePath =>
                    Path.GetFileName(filePath).StartsWith($"{surname}-") ||
                    Path.GetFileName(filePath).StartsWith($"{surname}.{name}-") ||
                    Path.GetFileName(filePath).StartsWith(Transliterate($"{surname}-")) ||
                    Path.GetFileName(filePath).StartsWith(Transliterate($"{surname}.{name}-")))
            .ToList();
    }

    private static bool VerifyStudentFile(StudentWork work, string filePath)
    {
        var fileName = Path.GetFileName(filePath);

        if (fileName[^4..] != ".pdf")
        {
            return false;
        }

        var index = fileName.IndexOf('-');
        if (index == -1)
        {
            return false;
        }

        var fileType = fileName.Substring(index + 1, fileName.Length - 5 - index);
        switch (fileType)
        {
            case "отчёт":
            case "отче\u0308т":
            case "report":
                work.HasReport = true;
                break;
            case "презентация":
            case "presentation":
                work.HasPresentation = true;
                break;
            case "отзыв":
            case "advisor-review":
                work.HasSupervisorReview = true;
                break;
            case "отзыв-консультанта":
            case "consultant-review":
                work.HasConsultantReview = true;
                break;
            case "рецензия":
            case "reviewer-review":
                work.HasReviewerReview = true;
                break;
            default:
                return false;
        }

        return true;
    }

    private static string Transliterate(string cyrillic)
    {
        var transliterationMap = new Dictionary<char, string>
        {
            { 'А', "A" }, { 'Б', "B" }, { 'В', "V" }, { 'Г', "G" }, { 'Д', "D" },
            { 'Е', "E" }, { 'Ё', "Yo" }, { 'Ж', "Zh" }, { 'З', "Z" }, { 'И', "I" },
            { 'Й', "I" }, { 'К', "K" }, { 'Л', "L" }, { 'М', "M" }, { 'Н', "N" },
            { 'О', "O" }, { 'П', "P" }, { 'Р', "R" }, { 'С', "S" }, { 'Т', "T" },
            { 'У', "U" }, { 'Ф', "F" }, { 'Х', "Kh" }, { 'Ц', "Ts" }, { 'Ч', "Ch" },
            { 'Ш', "Sh" }, { 'Щ', "Shch" }, { 'Ъ', string.Empty }, { 'Ы', "Y" }, { 'Ь', string.Empty },
            { 'Э', "E" }, { 'Ю', "Yu" }, { 'Я', "Ya" },
            { 'а', "a" }, { 'б', "b" }, { 'в', "v" }, { 'г', "g" }, { 'д', "d" },
            { 'е', "e" }, { 'ё', "yo" }, { 'ж', "zh" }, { 'з', "z" }, { 'и', "i" },
            { 'й', "i" }, { 'к', "k" }, { 'л', "l" }, { 'м', "m" }, { 'н', "n" },
            { 'о', "o" }, { 'п', "p" }, { 'р', "r" }, { 'с', "s" }, { 'т', "t" },
            { 'у', "u" }, { 'ф', "f" }, { 'х', "kh" }, { 'ц', "ts" }, { 'ч', "ch" },
            { 'ш', "sh" }, { 'щ', "shch" }, { 'ъ', string.Empty }, { 'ы', "y" }, { 'ь', string.Empty },
            { 'э', "e" }, { 'ю', "yu" }, { 'я', "ya" },
        };

        var result = new StringBuilder();

        foreach (var key in cyrillic)
        {
            if (transliterationMap.TryGetValue(key, out var value))
            {
                result.Append(value);
            }
            else
            {
                result.Append(key);
            }
        }

        return result.ToString();
    }
}