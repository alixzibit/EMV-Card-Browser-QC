﻿using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Collections.Generic;


namespace EMV_Card_Browser
{
public class ReportGenerator
{
    private readonly string _title;
    private readonly string _userName;
    private readonly IEnumerable<CardRecord> _records; // Assuming CardRecord is the model you're using

    public ReportGenerator(string title, string userName, IEnumerable<CardRecord> records)
    {
        _title = title;
        _userName = userName;
        _records = records;
    }

    public string GenerateReport()
    {
        PdfDocument document = new PdfDocument();
        document.Info.Title = _title;
        PdfPage page = document.AddPage();
        XGraphics gfx = XGraphics.FromPdfPage(page);
        XFont font = new XFont("Verdana", 20, XFontStyle.Bold);

        gfx.DrawString(_title, font, XBrushes.Black,
            new XRect(0, 0, page.Width, page.Height),
            XStringFormats.Center);

        font = new XFont("Verdana", 12, XFontStyle.Regular);
        gfx.DrawString($"Cards Chip QC report was generated by {_userName} and following is the cards which were read and verified to have successful personalized data:",
            font, XBrushes.Black, new XRect(20, 50, page.Width - 40, page.Height - 40));

        // Add logic to draw the data from _records to the PDF here

        string filename = "CardsQCReport.pdf";
        document.Save(filename);

        return filename;
    }
}
}