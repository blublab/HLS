using ApplicationCore.BuchhaltungKomponente.AccessLayer;
using ApplicationCore.BuchhaltungKomponente.DataAccessLayer;
using ApplicationCore.GeschaeftspartnerKomponente.AccessLayer;
using ApplicationCore.GeschaeftspartnerKomponente.DataAccessLayer;
using ApplicationCore.TransportplanungKomponente.DataAccessLayer;
using Common.DataTypes;
using Common.Implementations;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.PDFErzeugungsKomponente.AccesLayer
{
    public class PDFErzeugungKomponenteFacade : IPDFErzeugungsServicesFuerBuchhaltung
    {
        private static string logoPath = @"PdfData\HLS-Logo.png";
        private readonly IGeschaeftspartnerServicesFuerPDFErzeugung geschaeftspartnerServiceFuerPDFErzeugung;

        public PDFErzeugungKomponenteFacade(IGeschaeftspartnerServicesFuerPDFErzeugung geschaeftspartnerServicesFuerPDFErzeugung)
        {
            this.geschaeftspartnerServiceFuerPDFErzeugung = geschaeftspartnerServicesFuerPDFErzeugung;
        }

        private string[] ErstelleKundenanschrift(AdresseDTO adresse)
        {
            string[] result = new string[5] 
            {
                "Straße: " + adresse.Strasse,
                "Hausnummer: " + adresse.Hausnummer,
                "PLZ: " + adresse.PLZ,
                "Wohnort: " + adresse.Wohnort,
                "Land: " + adresse.Land
            };
            return result;
        }

        private string[] ErstelleTransportplanschrittkosten(IList<TransportplanSchrittDTO> tpSchritte)
        {
            string[] result = new string[tpSchritte.Count];
            int i = 0;
            foreach (TransportplanSchrittDTO tpsDTO in tpSchritte)
            {
                decimal wert = Math.Round(tpsDTO.Kosten, 2);
                result[i] = wert + "";
                i++;
            }
            return result;
        }

        private void SpeicherePDF(KundenrechnungDTO krDTO, string[] kundenadresse, string gesamtsumme, string erstellungsdatum, string[] tpSchritte)
        {
            // Dokument
            PdfDocument pdfDocument = new PdfDocument();
            pdfDocument.Info.Title = @"Kundenrechnung für Rechnung Nr. " + krDTO.RechnungsNr;

            // Page
            PdfPage pdfPage = pdfDocument.AddPage();

            // XGraphics
            XGraphics gfx = XGraphics.FromPdfPage(pdfPage);

            // Create a font
            XFont font = new XFont("Verdana", 10, XFontStyle.Regular);

            // Logo
            try
            {
                XImage image = XImage.FromFile(logoPath);
                double x = (1000 - (image.PixelWidth * (72 / image.HorizontalResolution))) / 2;
                gfx.DrawImage(image, x, 50);

                //XImage xImage = XImage.FromFile(logoPath);
                //gfx.DrawImage(xImage, 10, 10, xImage.PixelWidth, xImage.PixelWidth);
            }
            catch (Exception)
            {
            }

            // Kundenanschrift
            int y = 100;
            foreach (string str in kundenadresse)
            {
                gfx.DrawString(str, font, XBrushes.Black, new XRect(50, y, pdfPage.Width, pdfPage.Height), XStringFormats.TopLeft);
                y += 10;
            }

            // Erstellungsdatum
            gfx.DrawString(erstellungsdatum, font, XBrushes.Black, new XRect(200, 200, pdfPage.Width, pdfPage.Height), XStringFormats.TopCenter);

            // Transportplanschrittkosten
            XFont font3 = new XFont("Verdana", 11, XFontStyle.Italic);
            int z = 0;
            foreach (string str in tpSchritte)
            {
                gfx.DrawString("Platzhalter Start - Platzhalter Ende          " + str + "€", font3, XBrushes.Black, new XRect(0, z, pdfPage.Width, pdfPage.Height), XStringFormats.Center);
                z += 80;
            }

            //Horizontale Linie#
            int j = -220;
            for (int i = 0; i < 80; i++)
            {
                gfx.DrawString("_", font, XBrushes.Black, new XRect(j, 200, pdfPage.Width, pdfPage.Height), XStringFormats.Center);
                j += 3;
            }

            // Gesamtkosten
            XFont font2 = new XFont("Verdana", 16, XFontStyle.Bold);
            gfx.DrawString("Gesamtkosten: " + gesamtsumme.ToString() + "€", font2, XBrushes.Black, new XRect(-100, 220, pdfPage.Width, pdfPage.Height), XStringFormats.Center);

            // Speichere das Dokument
            pdfDocument.Save(Environment.CurrentDirectory + @"\BLUB.pdf");

            // Öffne das PDF mit dem default pdfViewer
            //Process.Start(target);   
        }

        #region IPDFErzeugungsServicesFuerBuchhaltung
        public void ErstelleKundenrechnungPDF(ref KundenrechnungDTO krDTO, IList<TransportplanSchrittDTO> tpSchritte)
        {
            int adNr = krDTO.Rechnungsadresse;
            AdresseDTO adrDTO = geschaeftspartnerServiceFuerPDFErzeugung.FindeAdresseZuID(adNr);
            WaehrungsType gesamtKosten = krDTO.Rechnungsbetrag;
            decimal gkosten = Math.Round(gesamtKosten.Wert, 2);
            DateTime erstellungsDatum = DateTime.Now;

            string[] kundenadresse = ErstelleKundenanschrift(adrDTO);
            string[] tpsStr = ErstelleTransportplanschrittkosten(tpSchritte);
            SpeicherePDF(krDTO, kundenadresse, gkosten.ToString(), erstellungsDatum.ToString(), tpsStr);
        }
        #endregion
    }
}