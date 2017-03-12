using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using RwOrders.Models;
using System.IO;
using System.Web;

namespace RwOrders.Reports
{
    public static class ConsignmentNoteReport
    {
        public static byte[] GeneratePdf(Consignment co)
        {
            // Create a MigraDoc document
            Document document = new Document();
            document.DefaultPageSetup.LeftMargin = Unit.FromCentimeter(1);
            document.DefaultPageSetup.RightMargin = Unit.FromCentimeter(1);
            MigraDoc.DocumentObjectModel.Style style = document.Styles["Normal"];
            style.Font.Name = "Arial";
            style.Font.Size = 10;
            Unit pageWidth = document.DefaultPageSetup.PageWidth;
            pageWidth -= document.DefaultPageSetup.LeftMargin + document.DefaultPageSetup.RightMargin;
            Section section = document.AddSection();
            section.PageSetup.HeaderDistance = Unit.FromMillimeter(5);
            section.PageSetup.TopMargin = Unit.FromCentimeter(7.5);

            // Add header
            section.PageSetup.StartingNumber = 1;
            HeaderFooter header = section.Headers.Primary;

            Paragraph title = new Paragraph();
            title.Format.Alignment = ParagraphAlignment.Center;
            title.Format.Font.Bold = true;
            title.Format.Font.Size = 32;
            title.AddText("CONSIGNMENT NOTE");
            header.Add(title);

            Table tableHeader = new Table();
            tableHeader.Borders.Width = 0.75;
            tableHeader.TopPadding = "1mm";
            tableHeader.BottomPadding = "1mm";

            tableHeader.AddColumn("7cm");
            tableHeader.AddColumn("4.2cm");
            tableHeader.AddColumn("2cm");
            tableHeader.AddColumn("2.2cm");
            tableHeader.AddColumn("3cm");

            Row headerRow = tableHeader.AddRow();
            tableHeader.AddRow();
            tableHeader.AddRow();
            tableHeader.AddRow();
            tableHeader.AddRow();
            tableHeader.AddRow();
            tableHeader.AddRow();
            headerRow.Cells[0].MergeDown = 6;
            headerRow.Cells[1].MergeRight = 1;
            headerRow.Cells[3].MergeRight = 1;
            tableHeader.Rows[1].Cells[1].MergeRight = 3;
            tableHeader.Rows[2].Cells[2].MergeRight = 2;
            tableHeader.Rows[3].Cells[2].MergeRight = 1;
            tableHeader.Rows[4].Cells[1].MergeRight = 3;
            tableHeader.Rows[5].Cells[1].MergeRight = 3;
            tableHeader.Rows[6].Cells[1].MergeRight = 3;

            //Paragraph logo = new Paragraph();
            //logo.Format.Alignment = ParagraphAlignment.Left;
            //logo.AddImage(HttpContext.Current.Server.MapPath("~/Images/MyImage.png")).Width = "7cm";
            //headerRow.Cells[0].Add(logo);

            Paragraph campus = headerRow.Cells[1].AddParagraph();
            campus.AddFormattedText("Campus: ", TextFormat.Bold);
            campus.AddText(co.Campus);

            Paragraph locality = headerRow.Cells[3].AddParagraph();
            locality.AddFormattedText("Locality: ", TextFormat.Bold);
            locality.AddText(co.Locality);

            Paragraph contactName = tableHeader.Rows[1].Cells[1].AddParagraph();
            contactName.AddFormattedText("Contact Name: ", TextFormat.Bold);
            contactName.AddText(co.ContactName);

            Paragraph phone = tableHeader.Rows[2].Cells[1].AddParagraph();
            phone.AddFormattedText("Phone: ", TextFormat.Bold);
            phone.AddText("");

            Paragraph email = tableHeader.Rows[2].Cells[2].AddParagraph();
            email.AddFormattedText("Email: ", TextFormat.Bold);
            email.AddText(co.Email);

            Paragraph dateSent = tableHeader.Rows[3].Cells[1].AddParagraph();
            dateSent.AddFormattedText("Date sent: ", TextFormat.Bold);
            dateSent.AddText(co.DispatchDate.ToString("dd/MM/yyyy"));

            Paragraph returnBy = tableHeader.Rows[3].Cells[2].AddParagraph();
            returnBy.AddFormattedText("Return by: ", TextFormat.Bold);
            returnBy.AddText(co.ReturnBy.ToString("dd/MM/yyyy"));

            Paragraph refNum = tableHeader.Rows[3].Cells[4].AddParagraph();
            refNum.AddFormattedText("Ref: ", TextFormat.Bold);
            refNum.AddText(co.ID.ToString());

            Paragraph deliveredTo = tableHeader.Rows[4].Cells[1].AddParagraph();
            deliveredTo.AddFormattedText("Delivered to: ", TextFormat.Bold);
            deliveredTo.AddText(co.CompanyName ?? "");

            tableHeader.Rows[5].Cells[1].AddParagraph().AddFormattedText("Unsold goods to be returned to Widgets Co", TextFormat.Bold);
            tableHeader.Rows[6].Cells[1].AddParagraph(@"Queries to enquiries@example.com
Unsold goods to be returned to Widgets Co Returns");

            header.Add(tableHeader);


            // Add main body
            Table tableItems = new Table();
            tableItems.Borders.Width = 0.75;
            tableItems.AddColumn("3cm");
            tableItems.AddColumn("8cm");
            tableItems.AddColumn("2.5cm");
            tableItems.AddColumn("1.5cm");
            tableItems.AddColumn("1.5cm");
            tableItems.AddColumn("1.5cm");
            tableItems.TopPadding = "3mm";
            tableItems.BottomPadding = "3mm";

            Row itemsHeader = tableItems.AddRow();
            itemsHeader.Format.Alignment = ParagraphAlignment.Center;
            itemsHeader.Format.Font.Bold = true;
            itemsHeader[0].AddParagraph("CODE");
            itemsHeader[1].AddParagraph("DESCRIPTION");
            itemsHeader[2].AddParagraph("PRICE");
            itemsHeader[3].AddParagraph("QTY");
            itemsHeader[4].AddParagraph("???");
            itemsHeader[5].AddParagraph("???");

            // Add each item
            foreach (var item in co.ConsignmentItems)
            {
                Row itemRow = tableItems.AddRow();
                itemRow[2].Format.Alignment = ParagraphAlignment.Right;
                itemRow[3].Format.Alignment = ParagraphAlignment.Center;
                itemRow[0].AddParagraph(item.StockCode ?? "");
                itemRow[1].AddParagraph(item.Description ?? "");
                itemRow[2].AddParagraph(item.UnitPrice.ToString("£#,##0.00"));
                itemRow[3].AddParagraph(item.Quantity.ToString());
            }
            section.Add(tableItems);


            PdfDocumentRenderer renderer = new PdfDocumentRenderer(true, PdfSharp.Pdf.PdfFontEmbedding.Always);
            renderer.Document = document;

            renderer.RenderDocument();

            MemoryStream ms = new MemoryStream();
            renderer.PdfDocument.Save(ms);
            return ms.ToArray();
        }
    }
}