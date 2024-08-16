// <copyright file="StudentWork.cs" company="Maria Myasnikova">
// Copyright (c) Maria Myasnikova. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace ScheduleParser.Models;

/// <summary>
/// Class containing information about student's work.
/// </summary>
public class StudentWork(int number, string studentName, string theme, string supervisor, string reviewer)
{
    /// <summary>
    /// Gets the student's number in the table.
    /// </summary>
    public int Number { get; } = number;

    /// <summary>
    /// Gets the student name.
    /// </summary>
    public string StudentName { get; } = studentName;

    /// <summary>
    /// Gets the theme of the student's work.
    /// </summary>
    public string Theme { get; } = theme;

    /// <summary>
    /// Gets the supervisor of the student's work.
    /// </summary>
    public string Supervisor { get; } = supervisor;

    /// <summary>
    /// Gets or sets the consultant of the student's work.
    /// </summary>
    public string? Consultant { get; set; }

    /// <summary>
    /// Gets the reviewer of the student's work.
    /// </summary>
    public string Reviewer { get; } = reviewer;

    /// <summary>
    /// Gets or sets a value indicating whether student has a report file.
    /// </summary>
    public bool HasReport { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether student has a presentation file.
    /// </summary>
    public bool HasPresentation { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether student has a supervisor's review file.
    /// </summary>
    public bool HasSupervisorReview { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether student has a consultant's review file.
    /// </summary>
    public bool HasConsultantReview { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether student has a reviewer's review file.
    /// </summary>
    public bool HasReviewerReview { get; set; } = false;
}