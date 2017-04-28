﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using EPPlusTutorial.Util;

namespace EPPlusTutorial
{
    [TestFixture]
    public class QuickTutorial
    {
        [Test]
        public void BasicUsage()
        {
            using (var package = new ExcelPackage())
            {
                ExcelWorksheet sheet = package.Workbook.Worksheets.Add("MySheet");
                ExcelRange firstCell = sheet.Cells[1, 1]; // or use "A1"
                firstCell.Value = "will it work...";
                sheet.Cells.AutoFitColumns();
                package.SaveAs(new FileInfo(BinDir.GetPath()));
            }
        }

        [Test]
        public void LoadingAndSaving()
        {
            // Open an existing Excel
            // Or if the file does not exist, create a new one
            using (var package = new ExcelPackage(new FileInfo(BinDir.GetPath()), "optionalPassword"))
            using (var basicUsageExcel = File.Open(BinDir.GetPath(nameof(BasicUsage)), FileMode.Open))
            {
                var sheet = package.Workbook.Worksheets.Add("Sheet1");
                sheet.Cells["D1"].Value = "Everything in the package will be overwritten";
                sheet.Cells["D2"].Value = "by the package.Load() below!!!";

                // Loads the worksheets from BasicUsage
                // (MySheet with A1 = will it work...)
                package.Load(basicUsageExcel);

                // See 3-Import for more loading techniques

                package.Compression = CompressionLevel.BestSpeed;
                package.Save("optionalPassword");
                //package.SaveAs(FileInfo / Stream)
                //Byte[] p = package.GetAsByteArray();
            }
        }

        [Test]
        public void SelectingCells()
        {
            using (var package = new ExcelPackage())
            {
                var sheet = package.Workbook.Worksheets.Add("MySheet");

                // One cell
                ExcelRange cellA2 = sheet.Cells["A2"];
                var alsoCellA2 = sheet.Cells[2, 1];
                Assert.That(cellA2.Address, Is.EqualTo("A2"));
                Assert.That(cellA2.Address, Is.EqualTo(alsoCellA2.Address));

                // Get the column from a cell
                // ExcelRange.Start is the top and left most cell
                Assert.That(cellA2.Start.Column, Is.EqualTo(1));
                // To really get the column: sheet.Column(1)

                // A range
                ExcelRange ranger = sheet.Cells["A2:C5"];
                var sameRanger = sheet.Cells[2, 1, 5, 3];
                Assert.That(ranger.Address, Is.EqualTo(sameRanger.Address));

                //sheet.Cells["A1,A4"] // Just A1 and A4
                //sheet.Cells["1:1"] // A row
                //sheet.Cells["A:B"] // Two columns

                // Linq
                var l = sheet.Cells["A1:A5"].Where(range => range.Comment != null);

                // Dimensions used
                Assert.That(sheet.Dimension, Is.Null);

                ranger.Value = "pushing";
                var usedDimensions = sheet.Dimension;
                Assert.That(usedDimensions.Address, Is.EqualTo(ranger.Address));

                // Offset: down 5 rows, right 10 columns
                var movedRanger = ranger.Offset(5, 10);
                Assert.That(movedRanger.Address, Is.EqualTo("K7:M10"));
                movedRanger.Value = "Moved";

                package.SaveAs(new FileInfo(BinDir.GetPath()));
            }
        }

        [Test]
        public void WritingValues()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            using (var package = new ExcelPackage())
            {
                var sheet = package.Workbook.Worksheets.Add("MySheet");

                // Format as text
                sheet.Cells["A1"].Style.Numberformat.Format = "@";

                // Numbers
                sheet.SetValue("A1", "Numbers");
                Assert.That(sheet.GetValue<string>(1, 1), Is.EqualTo("Numbers"));
                sheet.Cells["B1"].Value = 15.32;
                sheet.Cells["B1"].Style.Numberformat.Format = "#,##0.00";
                Assert.That(sheet.Cells["B1"].Text, Is.EqualTo("15.32"));

                // Percentage
                sheet.Cells["C1"].Value = 0.5;
                sheet.Cells["C1"].Style.Numberformat.Format = "0%";
                Assert.That(sheet.Cells["C1"].Text, Is.EqualTo("50%"));

                // Money
                sheet.Cells["A2"].Value = "Moneyz";
                sheet.Cells["B2,D2"].Value = 15000.23D;
                sheet.Cells["C2,E2"].Value = -2000.50D;
                sheet.Cells["B2:C2"].Style.Numberformat.Format = "#,##0.00 [$€-813];[RED]-#,##0.00 [$€-813]";
                sheet.Cells["D2:E2"].Style.Numberformat.Format = "[$$-409]#,##0";

                // DateTime
                sheet.Cells["A3"].Value = "Timey Wimey";
                sheet.Cells["B3"].Style.Numberformat.Format = "yyyy-mm-dd";
                sheet.Cells["B3"].Formula = $"=DATE({DateTime.Now:yyyy,MM,dd})";
                sheet.Cells["C3"].Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.FullDateTimePattern;
                sheet.Cells["C3"].Value = DateTime.Now;
                sheet.Cells["D3"].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";
                sheet.Cells["D3"].Value = DateTime.Now;


                // An external hyperlink
                sheet.Cells["C24"].Hyperlink = new Uri("http://pongit.be", UriKind.Absolute);
                sheet.Cells["C24"].Value = "Visit us";
                sheet.Cells["C24"].Style.Font.Color.SetColor(Color.Blue);
                sheet.Cells["C24"].Style.Font.UnderLine = true;

                //sheet.Cells["C25"].Formula = "HYPERLINK(\"mailto:support@pongit.be\",\"Contact support\")";
                //package.Workbook.Properties.HyperlinkBase = new Uri("");

                // An internal hyperlink
                package.Workbook.Worksheets.Add("Data");
                sheet.Cells["C26"].Hyperlink = new ExcelHyperLink("Data!A1", "Goto data sheet");

                sheet.Cells["Z1"].Clear();

                sheet.Cells.AutoFitColumns();
                package.SaveAs(new FileInfo(BinDir.GetPath()));
            }
        }

        [Test]
        public void FormattingCells()
        {
            using (var package = new ExcelPackage())
            {
                var sheet = package.Workbook.Worksheets.Add("Styling");

                // Cells with style
                ExcelFont font = sheet.Cells["A1"].Style.Font;
                sheet.Cells["A1"].Value = "Bold and proud";
                sheet.Cells["A1"].Style.Font.Name = "Arial";
                font.Bold = true;
                font.Color.SetColor(Color.Green);
                // ExcelFont also has: Size, Italic, Underline, Strike, ...

                sheet.Cells["A3"].Style.Font.SetFromFont(new Font(new FontFamily("Arial"), 15, FontStyle.Strikeout));
                sheet.Cells["A3"].Value = "SetFromFont(Font)";

                // Borders need to be made
                sheet.Cells["A1:A2"].Style.Border.BorderAround(ExcelBorderStyle.Dotted);
                sheet.Cells[5, 5, 9, 8].Style.Border.BorderAround(ExcelBorderStyle.Dotted);

                // Merge cells
                sheet.Cells[5, 5, 9, 8].Merge = true;

                // More style
                sheet.Cells["D14"].Style.ShrinkToFit = true;
                sheet.Cells["D14"].Style.Font.Size = 24;
                sheet.Cells["D14"].Value = "Shrinking for fit";

                sheet.Cells["D15"].Style.WrapText = true;
                sheet.Cells["D15"].Value = "A wrap, yummy!";
                sheet.Cells["D16"].Value = "No wrap, ouch!";

                // Setting a background color requires setting the PatternType first
                sheet.Cells["F6:G8"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells["F6:G8"].Style.Fill.BackgroundColor.SetColor(Color.Red);

                // Horizontal Alignment needs a little workaround
                // http://stackoverflow.com/questions/34660560/epplus-isnt-honoring-excelhorizontalalignment-center-or-right
                var centerStyle = package.Workbook.Styles.CreateNamedStyle("Center");
                centerStyle.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells["B5"].StyleName = "Center";
                sheet.Cells["B5"].Value = "I'm centered";

                // MIGHT NOT WORK:
                sheet.Cells["B6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells["B6"].Value = "I'm not centered? :(";

                package.SaveAs(new FileInfo(BinDir.GetPath()));
            }
        }

        [Test]
        public void FormattingSheetsAndColumns()
        {
            using (var package = new ExcelPackage())
            {
                var sheet = package.Workbook.Worksheets.Add("Victor");
                sheet.TabColor = Color.Ivory;

                // Freeze the top row and left 4 columns when scrolling
                sheet.View.FreezePanes(2, 5);

                sheet.View.ShowGridLines = false;
                sheet.View.ShowHeaders = false;
                
                //sheet.DeleteColumn();
                //sheet.InsertColumn();

                // Default selected cells when opening the xslx
                sheet.Select("B6");

                var colE = sheet.Column(5);
                //ExcelStyle colStyle = colE.Style; // See FormattingCells
                colE.AutoFit(); // or colE.Width

                // Who likes A's
                sheet.Column(1).Hidden = true;

                package.SaveAs(new FileInfo(BinDir.GetPath()));
            }
        }
    }
}
