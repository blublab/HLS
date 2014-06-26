using ApplicationCore.BuchhaltungKomponente.AccessLayer;
using ApplicationCore.BuchhaltungKomponente.DataAccessLayer;
using ApplicationCore.GeschaeftspartnerKomponente.AccessLayer;
using ApplicationCore.GeschaeftspartnerKomponente.DataAccessLayer;
using ApplicationCore.PDFErzeugungsKomponente.AccesLayer;
using ApplicationCore.TransportplanungKomponente.DataAccessLayer;
using Common.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util.PersistenceServices.Implementations;
using Util.PersistenceServices.Interfaces;
using Util.TimeServices;

namespace Test.Integrationtest
{
    [TestClass]
    public class IntegrationsTest_PDFErzeugungskomponente
    {
        private static IPersistenceServices persistenceService = null;
        private static ITransactionServices transactionService = null;

        private static IGeschaeftspartnerServicesFuerPDFErzeugung geschaeftspartnerServicesFuerPDFErzeugung = null;
        private static IPDFErzeugungsServicesFuerBuchhaltung pDFErzeungServicesFuerBuchhaltung = null;
        private static IGeschaeftspartnerServices geschaeftspartnerServices = null;

        [ClassInitialize]
        public static void InitializeTests(TestContext testContext)
        {
            log4net.Config.XmlConfigurator.Configure();

            PersistenceServicesFactory.CreateSimpleMySQLPersistenceService(out persistenceService, out transactionService);

            var timeServicesMock = new Mock<ITimeServices>();

            geschaeftspartnerServices = new GeschaeftspartnerKomponenteFacade(persistenceService, transactionService);
            geschaeftspartnerServicesFuerPDFErzeugung = geschaeftspartnerServices as IGeschaeftspartnerServicesFuerPDFErzeugung;
            pDFErzeungServicesFuerBuchhaltung = new PDFErzeugungKomponenteFacade(geschaeftspartnerServicesFuerPDFErzeugung);
            }

        [ClassCleanup]
        public static void CleanUpClass()
        {
        }

        [TestMethod, TestCategory("IntegrationsTest")]
        public void TestErstellePDFSuccess()
        {
            Adresse kundenadresse = new Adresse() { Strasse = "ABC-Strasse", Hausnummer = "123", Land = "Nimmerland", PLZ = "4567", Wohnort = "hinterm Baum" };
            IList<AdresseDTO> adressen = new List<AdresseDTO>();
            adressen.Add(kundenadresse.ToDTO());
            GeschaeftspartnerDTO gpDTO = new GeschaeftspartnerDTO()
            {
                Adressen = adressen,
                Email = new EMailType("hans.peter@rofl.net"),
                Vorname = "Hans",
                Nachname = "Peter",
            };
            geschaeftspartnerServices.CreateGeschaeftspartner(ref gpDTO);
            KundenrechnungDTO krDTO = new KundenrechnungDTO() 
            {
                 RechnungBezahlt = false,
                 Rechnungsadresse = 1, 
                 Rechnungsbetrag = new WaehrungsType(987654321), 
            };
            IList<TransportplanSchrittDTO> tpSchritte = new List<TransportplanSchrittDTO>();
            pDFErzeungServicesFuerBuchhaltung.ErstelleKundenrechnungPDF(ref krDTO, tpSchritte);
        }

        [TestCleanup]
        public void CleanUpTest()
        {
        }
    }
}
