using ApplicationCore.AuftragKomponente.AccessLayer;
using ApplicationCore.BuchhaltungKomponente.AccessLayer;
using ApplicationCore.SendungKomponente.AccessLayer;
using ApplicationCore.SendungKomponente.DataAccessLayer;
using ApplicationCore.TransportplanungKomponente.AccessLayer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util.PersistenceServices.Implementations;
using Util.PersistenceServices.Interfaces;

namespace Tests.KomponentenTest.SendungKomponente
{
    [TestClass]
    public class KomponentenTest_SendungKomponente
    {
        private static IPersistenceServices persistenceService = null;
        private static ITransactionServices transactionService = null;
        private static ISendungServices sendungServices = null;
        private static Mock<ITransportplanungServicesFuerSendung> transportplanungServicesFuerSendung = null;
        private static Mock<IAuftragServices> auftragServices = null;
        private static Mock<IBuchhaltungServicesFuerSendung> buchhaltungServicesFuerSendung = null;

        /// <summary>
        /// Initialize the persistence
        /// </summary>
        /// <param name="testContext">Testcontext provided by framework</param>
        [ClassInitialize]
        public static void InitializePersistence(TestContext testContext)
        {
            log4net.Config.XmlConfigurator.Configure();

            PersistenceServicesFactory.CreateSimpleMySQLPersistenceService(out persistenceService, out transactionService);
            transportplanungServicesFuerSendung = new Mock<ITransportplanungServicesFuerSendung>();
            auftragServices = new Mock<IAuftragServices>();
            buchhaltungServicesFuerSendung = new Mock<IBuchhaltungServicesFuerSendung>();
            sendungServices = new SendungKomponenteFacade(persistenceService, transactionService, transportplanungServicesFuerSendung.Object, auftragServices.Object, buchhaltungServicesFuerSendung.Object); 
        }

        [TestMethod]
        public void TestCreateSendungSuccess()
        {
            Sendung s = new Sendung()
            {
                SndNr = 1,
                SaNr = 100,
                TpNr = 1000
            };
            Assert.IsTrue(s != null, "Sendung nicht gefunden.");
            Assert.IsTrue(s.SndNr > 0, "SendungsNummer muss größer 0 sein.");
            Assert.IsTrue(s.SaNr > 0, "SendungsanfrageNummer muss größer 0 sein.");
            Assert.IsTrue(s.TpNr > 0, "TransportNummer muss größer 0 sein.");
        }

        [TestMethod]
        public void TestCreateSendungsverfolgungsereignisSuccess()
        {
            Sendungsverfolgungsereignis sve = new Sendungsverfolgungsereignis
            {
                Id = 1,
                Nachrichteninhalt = "Test",
                Ort = 1111,
                SNr = 1,
                Zeitpunkt = System.DateTime.Now
            };

            Assert.IsTrue(sve != null, "Sendungsverfolgungsereignis nicht gefunden.");
            Assert.IsTrue(sve.Id > 0, "Id muss größer 0 sein.");
            Assert.IsTrue(sve.Ort > 0, "Ort muss größer 0 sein.");
            Assert.IsTrue(sve.Zeitpunkt != null, "Zeitpunkt nicht gefunden");
        }
    }
}
