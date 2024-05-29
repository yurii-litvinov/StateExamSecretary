// <copyright file="CommissionMeeting.cs" company="Maria Myasnikova">
// Copyright (c) Maria Myasnikova. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace ScheduleParser.Models;

/// <summary>
/// Class containing information about one meeting of the commission.
/// </summary>
public class CommissionMeeting(string timeAndAuditorium, string meetingInfo, List<StudentWork> studentWorks)
{
    /// <summary>
    /// Gets the time and auditorium of the meeting.
    /// </summary>
    public string TimeAndAuditorium { get; } = timeAndAuditorium;

    /// <summary>
    /// Gets information about the meeting (chair, level of education, number of SEC).
    /// </summary>
    public string MeetingInfo { get; } = meetingInfo;

    /// <summary>
    /// Gets a list of the works of students participating in the meeting.
    /// </summary>
    public List<StudentWork> StudentWorks { get; } = studentWorks;
}