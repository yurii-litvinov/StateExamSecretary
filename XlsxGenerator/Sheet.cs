// <copyright file="Sheet.cs" company="Maria Myasnikova">
// Copyright (c) Maria Myasnikova. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace XlsxGenerator;

using NPOI.SS.UserModel;
using NPOI.SS.Util;

/// <summary>
/// Class representing a sheet.
/// </summary>
public class Sheet(ISheet sheet)
{
    private const int ScalingValue = 256;
    private const int DefaultSize = 1;

    /// <summary>
    /// Sets default height of the rows.
    /// </summary>
    public void SetDefaultRowHeight(int height) =>
        sheet.DefaultRowHeightInPoints = height;

    /// <summary>
    /// Sets the width of the specified column.
    /// </summary>
    public void SetColumnWidth(int index, int width) =>
        sheet.SetColumnWidth(index, width * ScalingValue);

    /// <summary>
    /// Automatically sets the width of columns with indexes in the range [start; end].
    /// </summary>
    public void AutosizeColumns(int start, int end)
    {
        for (var column = start; column <= end; column++)
        {
            sheet.AutoSizeColumn(column);
        }
    }

    /// <summary>
    /// Gets the index of last row.
    /// </summary>
    public int GetLastRowIndex() => sheet.LastRowNum;

    /// <summary>
    /// Merges cells in the range [firstRow; lastRow] x [firstCol; lastCol].
    /// </summary>
    public void MergeCells(int firstRow, int lastRow, int firstCol, int lastCol) =>
        sheet.AddMergedRegion(new CellRangeAddress(firstRow, lastRow, firstCol, lastCol));

    /// <summary>
    /// Writes to the selected row, starting from the selected column, with the specified formatting.
    /// By default, cells with plain text with general horizontal alignment and central vertical alignment
    /// are written to a new row from the first column.
    /// </summary>
    public void WriteRow(
        List<CellInfo> infos,
        bool isBold = false,
        bool isItalic = false,
        HorizontalAlignment horizontalAlignment = HorizontalAlignment.General,
        VerticalAlignment verticalAlignment = VerticalAlignment.Center,
        int rowIndex = -1,
        int columnIndex = 0)
    {
        var cellStyle = this.CreateCellStyle(isBold, isItalic, horizontalAlignment, verticalAlignment);

        rowIndex = this.NormalizeRowIndex(rowIndex);

        var row = sheet.GetRow(rowIndex) ?? sheet.CreateRow(rowIndex);

        foreach (var info in infos)
        {
            var cell = row.CreateCell(columnIndex);
            columnIndex += info.Width;
            cell.SetCellValue(info.Value);
            cell.CellStyle = cellStyle;

            if (info.Height + info.Width == 2 * DefaultSize)
            {
                continue;
            }

            while (rowIndex < cell.RowIndex + info.Height - 1)
            {
                rowIndex++;
                _ = sheet.GetRow(rowIndex) ?? sheet.CreateRow(rowIndex);
            }

            this.MergeCells(
                cell.RowIndex,
                cell.RowIndex + info.Height - 1,
                cell.ColumnIndex,
                cell.ColumnIndex + info.Width - 1);
        }
    }

    /// <summary>
    /// Writes to the selected column, starting from the selected row, with the specified formatting.
    /// By default, cells with plain text with general horizontal alignment and central vertical alignment
    /// are written to the first column from a new row.
    /// </summary>
    public void WriteColumn(
        List<CellInfo> infos,
        bool isBold = false,
        bool isItalic = false,
        HorizontalAlignment horizontalAlignment = HorizontalAlignment.General,
        VerticalAlignment verticalAlignment = VerticalAlignment.Center,
        int columnIndex = 0,
        int rowIndex = -1)
    {
        var cellStyle = this.CreateCellStyle(isBold, isItalic, horizontalAlignment, verticalAlignment);

        rowIndex = this.NormalizeRowIndex(rowIndex);

        foreach (var info in infos)
        {
            var row = sheet.GetRow(rowIndex) ?? sheet.CreateRow(rowIndex);
            var cell = row.CreateCell(columnIndex);
            rowIndex++;
            cell.SetCellValue(info.Value);
            cell.CellStyle = cellStyle;

            if (info.Height + info.Width == 2 * DefaultSize)
            {
                continue;
            }

            while (rowIndex < cell.RowIndex + info.Height)
            {
                _ = sheet.GetRow(rowIndex) ?? sheet.CreateRow(rowIndex);
                rowIndex++;
            }

            this.MergeCells(
                cell.RowIndex,
                cell.RowIndex + info.Height - 1,
                cell.ColumnIndex,
                cell.ColumnIndex + info.Width - 1);
        }
    }

    /// <summary>
    /// Writes a set number of empty rows, starting from the specified row of the table.
    /// By default, creates new empty rows.
    /// </summary>
    public void WriteEmptyRows(
        int count,
        int rowIndex = -1)
    {
        rowIndex = this.NormalizeRowIndex(rowIndex);

        for (var i = 0; i < count; i++)
        {
            sheet.CreateRow(rowIndex);
            rowIndex++;
        }
    }

    private ICellStyle CreateCellStyle(
        bool isBold,
        bool isItalic,
        HorizontalAlignment horizontalAlignment,
        VerticalAlignment verticalAlignment)
    {
        var font = sheet.Workbook.CreateFont();
        font.FontName = "Arial";
        font.FontHeightInPoints = 10;
        font.IsBold = isBold;
        font.IsItalic = isItalic;

        var cellStyle = sheet.Workbook.CreateCellStyle();
        cellStyle.SetFont(font);
        cellStyle.Alignment = horizontalAlignment;
        cellStyle.VerticalAlignment = verticalAlignment;
        cellStyle.WrapText = true;

        return cellStyle;
    }

    private int NormalizeRowIndex(int rowIndex)
    {
        if (rowIndex >= 0)
        {
            return rowIndex;
        }

        rowIndex = sheet.LastRowNum;
        if (sheet.GetRow(rowIndex) != null)
        {
            rowIndex++;
        }

        return rowIndex;
    }
}