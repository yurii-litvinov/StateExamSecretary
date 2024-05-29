// <copyright file="Config.cs" company="Maria Myasnikova">
// Copyright (c) Maria Myasnikova. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace ScheduleParser.Config;

using System.Text.Json.Serialization;

/// <summary>
/// Class for representing schedule and themes data from a JSON file.
/// </summary>
public class Config
{
    /// <summary>
    /// Gets or sets path or link to the file containing schedule.
    /// </summary>
    [JsonPropertyName("Расписание")]
    required public string Schedule { get; set; }

    /// <summary>
    /// Gets or sets path or link to the file containing themes of the BS of Programming technologies.
    /// </summary>
    [JsonPropertyName("Темы ВКР, бакалавры техпрога")]
    required public string BachelorsPt { get; set; }

    /// <summary>
    /// Gets or sets path or link to the file containing themes of the BS of Software engineering.
    /// </summary>
    [JsonPropertyName("Темы ВКР, бакалавры ПИ")]
    required public string BachelorsSe { get; set; }

    /// <summary>
    /// Gets or sets path or link to the file containing themes of the MSC of Programming technologies.
    /// </summary>
    [JsonPropertyName("Темы ВКР, магистры техпрога")]
    required public string MastersPt { get; set; }

    /// <summary>
    /// Gets or sets path or link to the file containing themes of the MSC of Software engineering.
    /// </summary>
    [JsonPropertyName("Темы ВКР, магистры ПИ")]
    required public string MastersSe { get; set; }
}