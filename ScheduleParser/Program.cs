// <copyright file="Program.cs" company="Maria Myasnikova">
// Copyright (c) Maria Myasnikova. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.
// </copyright>

#pragma warning disable SA1200
using System.Text.Json;
using ScheduleParser.Config;

#pragma warning restore SA1200

var jsonString = File.ReadAllText(Path.Combine("Config", "config.json"));

var config = JsonSerializer.Deserialize<Config>(jsonString);

var parser =
    new ScheduleParser.ScheduleParser(
        config ?? throw new InvalidOperationException("Файл конфигурации имеет неверный формат"));

var days = parser.Parse();

foreach (var day in days)
{
    Console.WriteLine(day.Date);
    foreach (var member in day.CommissionMembers)
    {
        Console.WriteLine(member);
    }
}