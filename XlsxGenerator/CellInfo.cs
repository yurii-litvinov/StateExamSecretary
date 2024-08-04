// <copyright file="CellInfo.cs" company="Maria Myasnikova">
// Copyright (c) Maria Myasnikova. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace XlsxGenerator;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Record that stores information about a cell.
/// </summary>
/// <param name="Value">Value contained in the cell.</param>
/// <param name="Width">Width of the cell (in the number of cells).</param>
/// <param name="Height">Height of the cell (in the number of cells).</param>
[SuppressMessage(
    "StyleCop.CSharp.NamingRules",
    "SA1313:Parameter names should begin with lower-case letter",
    Justification = "In the case of record, the parameter names must be capitalized.")]
public record CellInfo(string Value, int Width = 1, int Height = 1);