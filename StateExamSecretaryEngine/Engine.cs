// <copyright file="Engine.cs" company="Maria Myasnikova">
// Copyright (c) Maria Myasnikova. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace StateExamSecretaryEngine;

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
    private List<DaySchedule> days = [];
    private Config? config;

    /// <summary>
    /// Creates the file "Config/config.json" in the working directory, if it does not exist.
    /// </summary>
    public void CreateConfig()
    {
        const string fileName = "config.json";

        if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Config", fileName)))
        {
            return;
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
        };
        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
            WriteIndented = true,
        };
        var jsonString = JsonSerializer.Serialize(newConfig, options);

        Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Config"));
        File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "Config", fileName), jsonString);
    }

    /// <summary>
    /// Parse the schedule.
    /// </summary>
    public void ParseSchedule()
    {
        var jsonString = File.ReadAllText(Path.Combine("Config", "config.json"));

        this.config = JsonSerializer.Deserialize<Config>(jsonString);

        var parser =
            new ScheduleParser.ScheduleParser(
                this.config ?? throw new InvalidOperationException("Файл конфигурации имеет неверный формат"));

        this.days = parser.Parse();
    }

    /// <summary>
    /// Generates the orders of the day and save or upload them to disk.
    /// </summary>
    public void GenerateDayOrders()
    {
        var generator = new DayOrderGenerator();

        const string sesName = "Порядок дня для ГЭК";
        const string publicName = "Порядок дня для широкой публики";

        foreach (var day in this.days)
        {
            if (this.config is { SaveOrdersToDisk: true })
            {
                SaveToFile(
                    generator.GenerateSES(day),
                    Path.Combine(Directory.GetCurrentDirectory(), $"{sesName} ({day.Date}).xlsx"));
                SaveToFile(
                    generator.GeneratePublic(day),
                    Path.Combine(Directory.GetCurrentDirectory(), $"{publicName} ({day.Date}).xlsx"));
            }
        }
    }

    private static void SaveToFile(Stream stream, string path)
    {
        using var fileStream = new FileStream(path, FileMode.Create);
        stream.CopyTo(fileStream);
    }
}