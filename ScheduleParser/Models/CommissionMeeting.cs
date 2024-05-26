// <copyright file="CommissionMeeting.cs" company="Maria Myasnikova">
// Copyright (c) Maria Myasnikova. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace ScheduleParser.Models;

public class CommissionMeeting(string timeAndAuditorium, string meetingInfo, List<StudentWork> studentWorks)
{
    public string TimeAndAuditorium = timeAndAuditorium;
    public string MeetingInfo = meetingInfo;
    public List<StudentWork> StudentWorks = studentWorks;
}