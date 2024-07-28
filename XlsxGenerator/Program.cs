// <copyright file="Program.cs" company="Maria Myasnikova">
// Copyright (c) Maria Myasnikova. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.
// </copyright>

#pragma warning disable SA1200
using System.Text.Json;
using ScheduleParser.Config;
using XlsxGenerator;

#pragma warning restore SA1200

var jsonString = File.ReadAllText(Path.Combine("Config", "config.json"));

var config = JsonSerializer.Deserialize<Config>(jsonString);

var parser =
    new ScheduleParser.ScheduleParser(
        config ?? throw new InvalidOperationException("Файл конфигурации имеет неверный формат"));

var days = parser.Parse();

var generator = new DayOrderGenerator();

var directory = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;

if (directory == null)
{
    throw new DirectoryNotFoundException("Не удалость получить путь к папке");
}

const string sesName = "Порядок дня для ГЭК";

const string publicName = "Порядок дня для широкой публики";

foreach (var day in days)
{
    SaveToFile(generator.GenerateSES(day), Path.Combine(directory, $"{sesName} ({day.Date}).xlsx"));
    SaveToFile(generator.GeneratePublic(day), Path.Combine(directory, $"{publicName} ({day.Date}).xlsx"));
}

return;

void SaveToFile(Stream stream, string path)
{
    using var fileStream = new FileStream(path, FileMode.Create);
    stream.CopyTo(fileStream);
}