// <copyright file="StudentWork.cs" company="Maria Myasnikova">
// Copyright (c) Maria Myasnikova. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace ScheduleParser.Models;

public class StudentWork(int number, string studentName, string theme, string supervisor, string consultant, string reviewer)
{
    public int Number = number;
    public string StudentName = studentName;
    public string Theme = theme;
    public string Supervisor = supervisor;
    public string Consultant = consultant;
    public string Reviewer = reviewer;
}
