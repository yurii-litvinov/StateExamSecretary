// <copyright file="DaySchedule.cs" company="Maria Myasnikova">
// Copyright (c) Maria Myasnikova. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace ScheduleParser.Models;

public class DaySchedule(string date, List<string> commissionMembers, List<CommissionMeeting> commissionMeetings)
{
    public readonly string Date = date;
    public List<string> CommissionMembers = commissionMembers;
    public readonly List<CommissionMeeting> CommissionMeetings = commissionMeetings;
}