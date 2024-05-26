// <copyright file="Config.cs" company="Maria Myasnikova">
// Copyright (c) Maria Myasnikova. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace ScheduleParser.Config;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

/// <summary>
/// Class for representing data from a JSON file.
/// </summary>
[SuppressMessage(
    "StyleCop.CSharp.OrderingRules",
    "SA1206:Declaration keywords should follow order",
    Justification = "Conflict with the Rider linter.")]
public class Config
{
    /// <summary>
    /// Gets or sets path or link to the file containing schedule.
    /// </summary>
    [JsonPropertyName("Расписание")]
    public required string Schedule { get; set; }

    /// <summary>
    /// Gets or sets path or link to the file containing themes of the BS of Programming technologies.
    /// </summary>
    [JsonPropertyName("Темы ВКР, бакалавры техпрога")]
    public required string BachelorsPt { get; set; }

    /// <summary>
    /// Gets or sets path or link to the file containing themes of the BS of Software engineering.
    /// </summary>
    [JsonPropertyName("Темы ВКР, бакалавры ПИ")]
    public required string BachelorsSe { get; set; }

    /// <summary>
    /// Gets or sets path or link to the file containing themes of the MSC of Programming technologies.
    /// </summary>
    [JsonPropertyName("Темы ВКР, магистры техпрога")]
    public required string MastersPt { get; set; }

    /// <summary>
    /// Gets or sets path or link to the file containing themes of the MSC of Software engineering.
    /// </summary>
    [JsonPropertyName("Темы ВКР, магистры ПИ")]
    public required string MastersSe { get; set; }
}