using ApplicationCore.BuchhaltungKomponente.AccessLayer;
using ApplicationCore.BuchhaltungKomponente.DataAccessLayer;
using ApplicationCore.GeschaeftspartnerKomponente.AccessLayer;
using ApplicationCore.GeschaeftspartnerKomponente.DataAccessLayer;
using ApplicationCore.PDFErzeugungsKomponente.AccesLayer;
using Common.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.KomponentenTest.PDFErzeugungsKomponente
{
    [TestClass]
    public class KomponentenTest_PDFErzeugungsKomponente
    {
        private static KundenrechnungDTO krDTO = null;

        /// <summary>
        /// Initialize the PDFErzeugungs
        /// <param name="testContext">Testcontext provided by framework</param>
        /// </summary>
        [ClassInitialize]
        public static void InitializePDFErzeugung(TestContext testContext)
        {
            krDTO = new KundenrechnungDTO();
            krDTO.RechnungsNr = 123;
            krDTO.Rechnungsbetrag = new WaehrungsType(10);
            krDTO.RechnungBezahlt = false;
            ////krDTO.Zahlungseingang = new Zahlungseingang();
            ////krDTO.Sendungsanfrage = 1;
            ////krDTO.Rechnungsadressen = new List<int>();
        }

        [TestMethod]
        public void TestPDFErzeugung()
        {
            ////PDFErzeugungKomponenteFacade pdf = new PDFErzeugungKomponenteFacade(krDTO);

            ////pdf.ErstelleKundenanschrift(new AdresseDTO() { Strasse = "Müllweg", Hausnummer = "1", PLZ = "12345", Wohnort = "Müllstadt", Land = "Müllland" });

            ////pdf.ErstelleTransportplanschrittkosten("Hamburg", "München", new WaehrungsType(10), "Bollerwagen");
            ////pdf.ErstelleTransportplanschrittkosten("München", "Timbuktu", new WaehrungsType(5), "Storch");

            ////string file = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\asd.pdf";
            ////pdf.SpeicherePDF(file);
            ////Assert.IsTrue(File.Exists(file), "Datei existiert nicht");

            ////Adresse kundenadresse = new Adresse() { Strasse = "ABC-Strasse", Hausnummer = "123", Land = "Nimmerland", PLZ = "xyz", Wohnort = "hinterm Baum" }; 
            WaehrungsType betrag = new WaehrungsType(987654321);
            KundenrechnungDTO krDTO = new KundenrechnungDTO() { RechnungsNr = 1, RechnungBezahlt = false, Rechnungsadresse = 1, Rechnungsbetrag = betrag, Sendungsanfrage = 1 };
            Mock<IGeschaeftspartnerServicesFuerPDFErzeugung> geschaeftspartnerServicesFuerPDFErzeugung = new Mock<IGeschaeftspartnerServicesFuerPDFErzeugung>();
            IPDFErzeugungsServicesFuerBuchhaltung pDFErzeungServicesFuerBuchhaltung = new PDFErzeugungKomponenteFacade(geschaeftspartnerServicesFuerPDFErzeugung.Object);
            ////PDFErzeungServicesFuerBuchhaltung.ErstelleKundenrechnungPDF(krDTO, ;
        }
    }
}
