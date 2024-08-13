// <copyright file="Engine.cs" company="Maria Myasnikova">
// Copyright (c) Maria Myasnikova. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace StateExamSecretaryEngine;

using System.Text;
using System.Text.Unicode;
using System.Text.Encodings.Web;
using System.Diagnostics.CodeAnalysis;
using ScheduleParser.Models;
using XlsxGenerator;
using ScheduleParser.Config;
using System.Text.Json;

/// <summary>
/// Class responsible for business logic.
/// </summary>
[SuppressMessage(
    "StyleCop.CSharp.SpacingRules",
    "SA1010:OpeningSquareBracketsMustBeSpacedCorrectly",
    Justification = "Causes another problem with spaces.")]
public class Engine
{
    private readonly string mainFolderName = $"ГЭК {DateTime.Now.Year}";
    private List<DaySchedule> days = [];
    private Config? config;
    private IDiskWorker? diskWorker;

    /// <summary>
    /// Creates the file "Config/config.json" in the working directory, if it does not exist.
    /// </summary>
    /// <returns>Has the file been created or not.</returns>
    public bool TryCreateConfig()
    {
        const string fileName = "config.json";

        if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Config", fileName)))
        {
            this.SetConfig();
            return false;
        }

        const string plug = "путь или ссылка на файл";

        var newConfig = new Config
        {
            Schedule = plug,
            BachelorsPt = plug,
            BachelorsSe = plug,
            MastersPt = plug,
            MastersSe = plug,
            SaveOrdersToDisk = true,
            UploadFilesToDisk = true,
            Contents = "путь к папке",
            ClientId = string.Empty,
            ClientSecret = string.Empty,
            RedirectUri = string.Empty,
        };
        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
            WriteIndented = true,
        };
        var jsonString = JsonSerializer.Serialize(newConfig, options);

        Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Config"));
        File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "Config", fileName), jsonString);

        return true;
    }

    /// <summary>
    /// Parse the schedule.
    /// </summary>
    public void ParseSchedule()
    {
        var parser =
            new ScheduleParser.ScheduleParser(
                this.config ?? throw new InvalidOperationException("Файл конфигурации имеет неверный формат"));

        this.days = parser.Parse();
    }

    /// <summary>
    /// Generates the orders of the day and save or upload them to disk.
    /// </summary>
    public async Task GenerateDayOrders()
    {
        var generator = new DayOrderGenerator();

        foreach (var day in this.days)
        {
            await this.SaveOrUploadDayOrder(generator.GenerateSES(day), day.Date);
            await this.SaveOrUploadDayOrder(generator.GeneratePublic(day), day.Date);
        }
    }

    /// <summary>
    /// Checks the disk-related parameters to determine whether to use <see cref="YandexDiskWorker"/>.
    /// </summary>
    public bool CheckDiskParameters() =>
        this.config != null && (this.config.SaveOrdersToDisk || this.config.UploadFilesToDisk);

    /// <summary>
    /// Checks whether it is necessary to upload files to disk.
    /// </summary>
    public bool CheckUploadFilesToDisk() =>
        this.config is { UploadFilesToDisk: true };

    /// <summary>
    /// Gets access to the disk and creates a main folder on it.
    /// </summary>
    public async Task CreateMainFolder()
    {
        await this.SetDiskWorker();

        if (this.diskWorker != null)
        {
            await this.diskWorker.CreateFolder(this.mainFolderName);
        }
    }

    /// <summary>
    /// Uploads files to disk.
    /// </summary>
    public async Task UploadFilesToDisk()
    {
        if (this.config == null || this.diskWorker == null)
        {
            return;
        }

        foreach (var day in this.days)
        {
            var verifier = new StudentFileVerifier(day);
            var filePaths = verifier.VerifyFiles(this.config.Contents);

            var folderPath = $"{this.mainFolderName}/{day.Date}/Материалы";

            foreach (var path in filePaths)
            {
                await this.diskWorker.UploadFile(
                    File.Open(path, FileMode.Open),
                    $"{folderPath}/{Path.GetFileName(path)}");
            }
        }
    }

    /// <summary>
    /// Checks for files on the disk.
    /// </summary>
    /// <returns>Verification result report.</returns>
    public string CheckForFilesOnDisk()
    {
        if (this.diskWorker == null)
        {
            return string.Empty;
        }

        var report = new StringBuilder();

        foreach (var day in this.days)
        {
            var verifier = new StudentFileVerifier(day);

            var diskFilePaths = this.diskWorker
                .GetFolderFiles($"/{this.mainFolderName}/{day.Date}/Материалы").Result;

            report.Append(BuildReport(verifier.FindWorksWithMissingFiles(diskFilePaths)));
        }

        return report.ToString();
    }

    private static string BuildReport(IEnumerable<StudentWork> works)
    {
        var report = new StringBuilder();
        foreach (var work in works)
        {
            report.AppendLine($"{work.StudentName}:");
            if (!work.HasReport)
            {
                report.AppendLine("- нет отчёта");
            }

            if (!work.HasPresentation)
            {
                report.AppendLine("- нет презентации");
            }

            if (!work.HasSupervisorReview)
            {
                report.AppendLine("- нет отзыва научного руководителя");
            }

            if (!work.HasConsultantReview)
            {
                report.AppendLine("- нет отзыва консультанта");
            }

            if (!work.HasReviewerReview)
            {
                report.AppendLine("- нет рецензии");
            }
        }

        return report.ToString();
    }

    private static void SaveToFile(Stream stream, string path)
    {
        using var fileStream = new FileStream(path, FileMode.Create);
        stream.CopyTo(fileStream);
    }

    private async Task SaveOrUploadDayOrder(Stream stream, string date)
    {
        const string sesName = "Порядок дня для ГЭК";
        const string publicName = "Порядок дня для широкой публики";

        if (this.config is { SaveOrdersToDisk: true } && this.diskWorker != null)
        {
            var folderPath = $"{this.mainFolderName}/{date}";

            await this.diskWorker.CreateFolder(folderPath);
            await this.diskWorker.CreateFolder($"{folderPath}/Материалы");
            await this.diskWorker.CreateFolder($"{folderPath}/Документы");

            await this.diskWorker.UploadFile(stream, $"{folderPath}/{sesName} ({date}).xlsx");
            await this.diskWorker.UploadFile(stream, $"{folderPath}/{publicName} ({date}).xlsx");
        }
        else
        {
            const string folderName = "Порядки дня";

            Directory.CreateDirectory(folderName);
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), folderName);

            SaveToFile(stream, Path.Combine(folderPath, $"{sesName} ({date}).xlsx"));
            SaveToFile(stream, Path.Combine(folderPath, $"{publicName} ({date}).xlsx"));
        }
    }

    private void SetConfig()
    {
        var jsonString = File.ReadAllText(Path.Combine("Config", "config.json"));
        this.config = JsonSerializer.Deserialize<Config>(jsonString);
    }

    private async Task SetDiskWorker()
    {
        if (this.config != null)
        {
            this.diskWorker = new YandexDiskWorker();
            await this.diskWorker.GetOAuthToken(
                this.config.ClientId,
                this.config.ClientSecret,
                this.config.RedirectUri);
        }
    }
}