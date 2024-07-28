// <copyright file="CellInfo.cs" company="Maria Myasnikova">
// Copyright (c) Maria Myasnikova. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace XlsxGenerator;

/// <summary>
/// Class that stores information about a cell.
/// </summary>
public class CellInfo(string value, int width = 1, int height = 1)
{
    /// <summary>
    /// Gets the value contained in the cell.
    /// </summary>
    public string Value { get; } = value;

    /// <summary>
    /// Gets the width of the cell (in the number of cells).
    /// The default value = 1.
    /// </summary>
    public int Width { get; } = width;

    /// <summary>
    /// Gets the height of the cell (in the number of cells).
    /// The default value = 1.
    /// </summary>
    public int Height { get; } = height;
}