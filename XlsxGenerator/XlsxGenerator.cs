// <copyright file="XlsxGenerator.cs" company="Maria Myasnikova">
// Copyright (c) Maria Myasnikova. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace XlsxGenerator;

using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

/// <summary>
/// Class to generate .xlsx file.
/// </summary>
public class XlsxGenerator
{
    private readonly IWorkbook workbook = new XSSFWorkbook();

    /// <summary>
    /// Creates a new sheet with the specified name.
    /// If the name is not specified, it will be set by default.
    /// </summary>
    /// <returns>Instance of the <see cref="Sheet"/> class.</returns>
    public Sheet CreateSheet(string sheetName = "") =>
        sheetName == string.Empty
            ? new Sheet(this.workbook.CreateSheet())
            : new Sheet(this.workbook.CreateSheet(sheetName));

    /// <summary>
    /// Gets the sheet by its name.
    /// </summary>
    /// <returns>Instance of the <see cref="Sheet"/> class.</returns>
    public Sheet GetSheet(string sheetName) => new Sheet(this.workbook.GetSheet(sheetName));

    /// <summary>
    /// Gets all sheets.
    /// </summary>
    /// <returns>List sheets <see cref="Sheet"/>.</returns>
    public List<Sheet> GetAllSheets()
    {
        var sheets = new List<Sheet>();

        for (var i = 0; i < this.workbook.NumberOfSheets; i++)
        {
            sheets.Add(new Sheet(this.workbook.GetSheetAt(i)));
        }

        return sheets;
    }

    /// <summary>
    /// Gets .xlsx file as a stream.
    /// </summary>
    public Stream GetStream()
    {
        var memoryStream = new MemoryStream();
        this.workbook.Write(memoryStream, true);
        memoryStream.Position = 0;
        return memoryStream;
    }
}