using ApplicationCore.AuftragKomponente.AccessLayer;
using ApplicationCore.AuftragKomponente.DataAccessLayer;
using ApplicationCore.TransportnetzKomponente.AccessLayer;
using ApplicationCore.TransportnetzKomponente.DataAccessLayer;
using ApplicationCore.TransportplanungKomponente.AccessLayer;
using ApplicationCore.UnterbeauftragungKomponente.AccessLayer;
using ApplicationCore.UnterbeauftragungKomponente.DataAccessLayer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading;
using Util.PersistenceServices.Implementations;
using Util.PersistenceServices.Interfaces;
using Util.TimeServices;

namespace Tests.IntegrationsTest
{
    [TestClass]
    [DeploymentItem("Configurations/ConnectionStrings.config", "Configurations")]
    [DeploymentItem("NHibernate.ByteCode.Castle.dll")]
    [DeploymentItem("NHibernate.dll")]
    [DeploymentItem("NHibernate.Driver.MySqlDataDriver.dll")]
    [DeploymentItem("Mysql.Data.dll")]
    public class IntegrationsTest_Auftrag
    {
        private static IPersistenceServices persistenceService = null;
        private static ITransactionServices transactionService = null;

        private static ITransportplanungServices transportplanungsServices = null;
        private static IAuftragServices auftragsServices = null;
        private static ITransportnetzServices transportnetzServices = null;
        private static Mock<IFrachtfuehrerServicesFürUnterbeauftragung> frachtfuehrerServicesMock = null;

        private static LokationDTO hamburgLokation;
        private static LokationDTO bremerhavenLokation;
        private static TransportbeziehungDTO hh_bhv;

        [ClassInitialize]
        public static void InitializeTests(TestContext testContext)
        {
            log4net.Config.XmlConfigurator.Configure();

            PersistenceServicesFactory.CreateSimpleMySQLPersistenceService(out persistenceService, out transactionService);

            var timeServicesMock = new Mock<ITimeServices>();
            //// Wir müssen einen fixen Zeitpunkt simulieren, ansonsten sind bei der Ausführung/Planung evtl. die Verträge oder Angebote abgelaufen
            timeServicesMock.Setup(ts => ts.Now)
               .Returns(DateTime.Parse("31.08.2013 12:00"));

            transportnetzServices = new TransportnetzKomponenteFacade();
            auftragsServices = new AuftragKomponenteFacade(persistenceService, transactionService, timeServicesMock.Object);
            IAuftragServicesFürTransportplanung auftragsServicesFürTransportplanung = auftragsServices as IAuftragServicesFürTransportplanung;
            frachtfuehrerServicesMock = new Mock<IFrachtfuehrerServicesFürUnterbeauftragung>();
            IUnterbeauftragungServices unterbeauftragungsServices = new UnterbeauftragungKomponenteFacade(persistenceService, transactionService, frachtfuehrerServicesMock.Object);
            transportplanungsServices = new TransportplanungKomponenteFacade(persistenceService, transactionService, auftragsServicesFürTransportplanung, unterbeauftragungsServices as IUnterbeauftragungServicesFürTransportplanung, transportnetzServices as ITransportnetzServicesFürTransportplanung, timeServicesMock.Object);
            auftragsServicesFürTransportplanung.RegisterTransportplanungServiceFürAuftrag(transportplanungsServices as ITransportplanungServicesFürAuftrag);

            hamburgLokation = new LokationDTO("Hamburg", TimeSpan.Parse("10"), 10);
            bremerhavenLokation = new LokationDTO("Bremerhaven", TimeSpan.Parse("15"), 15);

            transportnetzServices.CreateLokation(ref hamburgLokation);
            transportnetzServices.CreateLokation(ref bremerhavenLokation);

            hh_bhv = new TransportbeziehungDTO(hamburgLokation, bremerhavenLokation);

            transportnetzServices.CreateTransportbeziehung(ref hh_bhv);

            FrachtfuehrerDTO frfHH_BHV = new FrachtfuehrerDTO();
            unterbeauftragungsServices.CreateFrachtfuehrer(ref frfHH_BHV); 
            FrachtfuehrerRahmenvertragDTO frv_hh_bhv = new FrachtfuehrerRahmenvertragDTO();
            frv_hh_bhv.GueltigkeitAb = DateTime.Parse("01.01.2013");
            frv_hh_bhv.GueltigkeitBis = DateTime.Parse("31.12.2013");
            frv_hh_bhv.Abfahrtszeiten = new System.Collections.Generic.List<StartzeitDTO>() 
            { 
                new StartzeitDTO() { Wochentag = DayOfWeek.Tuesday, Uhrzeit = 8 },
                new StartzeitDTO() { Wochentag = DayOfWeek.Wednesday, Uhrzeit = 8 },
                new StartzeitDTO() { Wochentag = DayOfWeek.Friday, Uhrzeit = 8 }
            };
            frv_hh_bhv.KapazitaetTEU = 4;
            frv_hh_bhv.KostenFix = 1000;
            frv_hh_bhv.KostenProTEU = 100;
            frv_hh_bhv.KostenProFEU = 200;
            frv_hh_bhv.FuerTransportAufTransportbeziehung = hh_bhv.TbNr;
            frv_hh_bhv.Frachtfuehrer = frfHH_BHV;
            frv_hh_bhv.Zeitvorgabe = TimeSpan.Parse("2"); // 2 Tage
            unterbeauftragungsServices.CreateFrachtfuehrerRahmenvertrag(ref frv_hh_bhv);
        }

        [ClassCleanup]
        public static void CleanUpClass()
        {
            transportnetzServices.DeleteTransportnetz();
        }

        [TestCleanup]
        public void CleanUpTest()
        {
            transactionService.ExecuteTransactional((_) =>
            {
                persistenceService.ExecuteSQLQuery("DELETE FROM FRACHTEINHEIT");
                persistenceService.ExecuteSQLQuery("DELETE FROM TRANSPORTAKTIVITAET");
                persistenceService.ExecuteSQLQuery("DELETE FROM TRANSPORTPLANSCHRITT");
                persistenceService.ExecuteSQLQuery("DELETE FROM TRANSPORTPLAN");
                persistenceService.ExecuteSQLQuery("DELETE FROM FRACHTAUFTRAG");
            });
        }

        [TestMethod, TestCategory("IntegrationsTest")]
        public void TestPlanungAngebotAblehnenSuccess()
        {
            SendungsanfrageDTO saDTO = new SendungsanfrageDTO();
            saDTO.Sendungspositionen.Add(new SendungspositionDTO());
            saDTO.AbholzeitfensterStart = DateTime.Parse("29.07.2013");
            saDTO.AbholzeitfensterEnde = DateTime.Parse("04.08.2013");
            saDTO.StartLokation = hamburgLokation.LokNr;
            saDTO.ZielLokation = bremerhavenLokation.LokNr;

            Assert.IsTrue(saDTO.SaNr == 0);
            auftragsServices.CreateSendungsanfrage(ref saDTO);
            Assert.IsTrue(saDTO.SaNr > 0);
            Assert.IsTrue(saDTO.Status == SendungsanfrageStatusTyp.Erfasst);
            Assert.IsTrue(transportplanungsServices.FindTransportplaeneZuSendungsanfrage(saDTO.SaNr).Count == 0);
            auftragsServices.PlaneSendungsanfrage(saDTO.SaNr);
            saDTO = auftragsServices.FindSendungsanfrage(saDTO.SaNr);
            Assert.IsTrue(saDTO.Status == SendungsanfrageStatusTyp.Geplant);
            Assert.IsTrue(transportplanungsServices.FindTransportplaeneZuSendungsanfrage(saDTO.SaNr).Count > 0);
            auftragsServices.LehneAngebotAb(saDTO.SaNr);
            saDTO = auftragsServices.FindSendungsanfrage(saDTO.SaNr);
            Assert.IsTrue(saDTO.Status == SendungsanfrageStatusTyp.Abgelehnt);
            Assert.IsTrue(transportplanungsServices.FindTransportplaeneZuSendungsanfrage(saDTO.SaNr).Count == 0);
        }

        [TestMethod, TestCategory("IntegrationsTest")]
        public void TestPlanungAngebotAbgelaufenSuccess()
        {
            SendungsanfrageDTO saDTO = new SendungsanfrageDTO();
            saDTO.Sendungspositionen.Add(new SendungspositionDTO());
            saDTO.AbholzeitfensterStart = DateTime.Parse("29.07.2013");
            saDTO.AbholzeitfensterEnde = DateTime.Parse("04.08.2013");
            saDTO.AngebotGültigBis = DateTime.Now;
            saDTO.StartLokation = hamburgLokation.LokNr;
            saDTO.ZielLokation = bremerhavenLokation.LokNr;

            auftragsServices.CreateSendungsanfrage(ref saDTO);
            auftragsServices.PlaneSendungsanfrage(saDTO.SaNr);
            saDTO = auftragsServices.FindSendungsanfrage(saDTO.SaNr);
            Assert.IsTrue(saDTO.Status == SendungsanfrageStatusTyp.Geplant);
            Assert.IsTrue(transportplanungsServices.FindTransportplaeneZuSendungsanfrage(saDTO.SaNr).Count > 0);
            auftragsServices.StartAngebotsGültigkeitsPrüfungPeriodicTask(new TimeSpan(0, 0, 2));
            Assert.IsTrue(
                SpinWait.SpinUntil(
                    () => 
                    {
                        return auftragsServices.FindSendungsanfrage(saDTO.SaNr).Status == SendungsanfrageStatusTyp.Abgelaufen;
                    }, 
                    10000), 
                "Angebot wurde nicht als abgelaufen markiert.");
            auftragsServices.StopAngebotsGültigkeitsPrüfungPeriodicTask();
            Assert.IsTrue(transportplanungsServices.FindTransportplaeneZuSendungsanfrage(saDTO.SaNr).Count == 0);
        }
    }
}
