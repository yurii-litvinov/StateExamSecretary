// <copyright file="DaySchedule.cs" company="Maria Myasnikova">
// Copyright (c) Maria Myasnikova. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace ScheduleParser.Models;

/// <summary>
/// Class containing information about one day of the schedule.
/// </summary>
public class DaySchedule(string date, List<string> commissionMembers, List<CommissionMeeting> commissionMeetings)
{
    /// <summary>
    /// Gets the date.
    /// </summary>
    public string Date { get; } = date;

    /// <summary>
    /// Gets a list of members of the commission participating in the meetings.
    /// </summary>
    public List<string> CommissionMembers { get; } = commissionMembers;

    /// <summary>
    /// Gets a list of commission meetings held on that day.
    /// </summary>
    public List<CommissionMeeting> CommissionMeetings { get; } = commissionMeetings;
}