using ApplicationCore.GeschaeftspartnerKomponente.AccessLayer;
using ApplicationCore.GeschaeftspartnerKomponente.DataAccessLayer;
using Common.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Util.PersistenceServices.Implementations;
using Util.PersistenceServices.Interfaces;

namespace Tests.KomponentenTest.GeschaeftspartnerKomponente
{
    [TestClass]
    public class KomponentenTest_GeschaeftspartnerKomponente
    {
        private static IPersistenceServices persistenceService = null;
        private static ITransactionServices transactionService = null;
        private static GeschaeftspartnerKomponenteFacade gpK_AC = null;

        /// <summary>
        /// Initialize the persistence
        /// </summary>
        /// <param name="testContext">Testcontext provided by framework</param>
        [ClassInitialize]
        public static void InitializePersistence(TestContext testContext)
        {
            log4net.Config.XmlConfigurator.Configure();

            PersistenceServicesFactory.CreateSimpleMySQLPersistenceService(out persistenceService, out transactionService);

            gpK_AC = new GeschaeftspartnerKomponenteFacade(persistenceService, transactionService);
        }

        [TestMethod]
        public void TestCreateGeschaeftspartnerSuccess()
        {
            AdresseDTO adresse = new AdresseDTO() { Strasse = "Hamburger Straße", Hausnummer = "1a", PLZ = "2000", Wohnort = "Hamburg", Land = "Deutschland" };
            EMailType testMail = new EMailType("max@mustermann.de");
            GeschaeftspartnerDTO gpDTO = new GeschaeftspartnerDTO() { Vorname = "Max", Nachname = "Mustermann", Email = testMail, Adressen = new List<AdresseDTO>() };
            gpDTO.Adressen.Add(adresse);
            Assert.IsTrue(gpDTO.GpNr == 0, "Id of Geschaeftspartner must be null.");
            gpK_AC.CreateGeschaeftspartner(ref gpDTO);
            Assert.IsTrue(gpDTO.Email == testMail, "EMail must be a valid EMail");
            Assert.IsTrue(gpDTO.GpNr > 0, "Id of Geschaeftspartner must not be 0.");
            Assert.IsTrue(gpDTO.Version > 0, "Version of Geschaeftspartner must not be 0.");
        }

        [TestMethod]
        public void TestUpdateGeschaeftspartnerSuccess()
        {
            EMailType testMail = new EMailType("max@mustermann.de");
            GeschaeftspartnerDTO gpDTO = new GeschaeftspartnerDTO() { Vorname = "Max", Nachname = "Mustermann", Email = testMail };
            gpK_AC.CreateGeschaeftspartner(ref gpDTO);

            gpDTO.Vorname = "Maria";
            gpK_AC.UpdateGeschaeftspartner(ref gpDTO);

            gpDTO = gpK_AC.FindGeschaeftspartner(gpDTO.GpNr);
            Assert.IsTrue(gpDTO != null, "Geschaeftspartner nicht gefunden.");
            Assert.IsTrue(gpDTO.Vorname == "Maria", "Geschaeftspartner nicht geändert.");
            Assert.IsTrue(gpDTO.Nachname == "Mustermann", "Geschaeftspartner nicht geändert.");
            Assert.IsTrue(gpDTO.Email == testMail, "EMail must be a valid EMail");
        }

        [TestMethod]
        public void TestFindGeschaeftspartnerByIdSuccess()
        {
            EMailType testMail = new EMailType("max@mustermann.de");
            GeschaeftspartnerDTO gpDTO1 = new GeschaeftspartnerDTO() { Vorname = "Heinz", Nachname = "Schmidt", Email = testMail };
            gpK_AC.CreateGeschaeftspartner(ref gpDTO1);
            GeschaeftspartnerDTO gpDTO2 = gpK_AC.FindGeschaeftspartner(gpDTO1.GpNr);
            Assert.IsTrue(gpDTO1.GpNr == gpDTO2.GpNr, "Geschaeftspartner must be the same.");
            Assert.IsTrue(gpDTO1.Email == testMail, "EMail must be a valid EMail");
        }

        [TestMethod]
        [ExpectedException(typeof(Util.PersistenceServices.Exceptions.OptimisticConcurrencyException))]
        public void TestUpdateGeschaeftspartnerFailWegenOptimisticConcurrencyException()
        {
            EMailType testMail = new EMailType("max@mustermann.de");
            GeschaeftspartnerDTO gpDTOOriginal = new GeschaeftspartnerDTO() { Vorname = "Maria", Nachname = "Mustermann", Email = testMail };
            gpK_AC.CreateGeschaeftspartner(ref gpDTOOriginal);
            Assert.IsTrue(gpDTOOriginal.GpNr > 0, "Id of Geschaeftspartner must not be 0.");
            Assert.IsTrue(gpDTOOriginal.Email == testMail, "EMail must be a valid EMail");

            AutoResetEvent syncEvent1 = new AutoResetEvent(false);
            AutoResetEvent syncEvent2 = new AutoResetEvent(false);
            Task task1 = Task.Factory.StartNew(() =>
            {
                GeschaeftspartnerDTO gpDTOTask = gpK_AC.FindGeschaeftspartner(gpDTOOriginal.GpNr);
                gpDTOTask.Nachname = "Musterfrau";
                gpK_AC.UpdateGeschaeftspartner(ref gpDTOTask);
                syncEvent1.WaitOne();
                syncEvent2.Set();
            });

            GeschaeftspartnerDTO gpDTO = gpK_AC.FindGeschaeftspartner(gpDTOOriginal.GpNr);
            gpDTO.Vorname = "Maria Janine";
            syncEvent1.Set();
            syncEvent2.WaitOne();
            gpK_AC.UpdateGeschaeftspartner(ref gpDTO);

            task1.Wait();
        }
    }
}
