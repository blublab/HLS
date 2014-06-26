using ApplicationCore.AuftragKomponente.AccessLayer;
using ApplicationCore.AuftragKomponente.DataAccessLayer;
using ApplicationCore.BankAdapter.AccessLayer;
using ApplicationCore.BuchhaltungKomponente.AccessLayer;
using ApplicationCore.FrachtfuehrerAdapter.AccessLayer;
using ApplicationCore.GeschaeftspartnerKomponente.AccessLayer;
using ApplicationCore.GeschaeftspartnerKomponente.DataAccessLayer;
using ApplicationCore.LokationsAdapter.AccessLayer;
using ApplicationCore.PDFErzeugungsKomponente.AccesLayer;
using ApplicationCore.SendungKomponente.AccessLayer;
using ApplicationCore.SendungKomponente.DataAccessLayer;
using ApplicationCore.TransportnetzKomponente.AccessLayer;
using ApplicationCore.TransportnetzKomponente.DataAccessLayer;
using ApplicationCore.TransportplanungKomponente.AccessLayer;
using ApplicationCore.TransportplanungKomponente.DataAccessLayer;
using ApplicationCore.UnterbeauftragungKomponente.AccessLayer;
using ApplicationCore.UnterbeauftragungKomponente.DataAccessLayer;
using Common.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Util.PersistenceServices.Implementations;
using Util.PersistenceServices.Interfaces;
using Util.TimeServices;

namespace Tests.Verbundtest
{
    [TestClass]
    public class VerbundTest_Transportverfolgung
    {
        private static IPersistenceServices persistenceService = null;
        private static ITransactionServices transactionService = null;

        private static ITransportplanungServices transportplanungsServices = null;
        private static ITransportplanungServicesFuerSendung transportplanungsServicesFuerSendung = null;
        private static IAuftragServices auftragsServices = null;
        private static IUnterbeauftragungServices unterbeauftragungsServices = null;
        private static ITransportnetzServices transportnetzServices = null;
        private static IFrachtfuehrerServicesFürUnterbeauftragung frachtfuehrerServices = null;
        private static IBuchhaltungServices buchhaltungServices = null;
        private static IUnterbeauftragungServicesFuerBuchhaltung unterbeauftragungsServicesFuerBuchhaltung = null;
        private static IBankServicesFuerBuchhaltung bankServicesFuerBuchhaltung = null;
        private static IGeschaeftspartnerServices geschaeftspartnerServices = null;

        private static ISendungServices sendungServices = null;

        private static LokationsApaterFacade lokationAdapter = null;

        private static LokationDTO hamburgLokation;
        private static LokationDTO bremerhavenLokation;
        private static LokationDTO shanghaiLokation;
        private static TransportbeziehungDTO hh_bhv;
        private static TransportbeziehungDTO bhv_sh;

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
            frachtfuehrerServices = new FrachtfuehrerAdapterFacade(ref buchhaltungServices);
            bankServicesFuerBuchhaltung = new BankAdapterFacade();
            geschaeftspartnerServices = new GeschaeftspartnerKomponenteFacade(persistenceService, transactionService);
            IGeschaeftspartnerServicesFuerPDFErzeugung geschaeftspartnerServicesFuerPDFErzeugung = new GeschaeftspartnerKomponenteFacade(persistenceService, transactionService);
            IPDFErzeugungsServicesFuerBuchhaltung pDFErzeugungsServicesFuerBuchhaltung = new PDFErzeugungKomponenteFacade(geschaeftspartnerServicesFuerPDFErzeugung);
            unterbeauftragungsServices = new UnterbeauftragungKomponenteFacade(persistenceService, transactionService, frachtfuehrerServices);
            IUnterbeauftragungServicesFuerBuchhaltung unterbeauftragungsServicesFuerBuchhaltung = new UnterbeauftragungKomponenteFacade(persistenceService, transactionService, frachtfuehrerServices);
            IUnterbeauftragungServicesFürTransportplanung unterbeauftragungServicesFürTransportplanung = new UnterbeauftragungKomponenteFacade(persistenceService, transactionService, frachtfuehrerServices);
            ITransportnetzServicesFürTransportplanung transportnetzServicesFürTransportplanung = new TransportnetzKomponenteFacade();
            transportplanungsServices = new TransportplanungKomponenteFacade(persistenceService, transactionService, auftragsServicesFürTransportplanung, unterbeauftragungServicesFürTransportplanung, transportnetzServicesFürTransportplanung, timeServicesMock.Object);
            ITransportplanServicesFuerBuchhaltung transportplanServicesFuerBuchhaltung = new TransportplanungKomponenteFacade(persistenceService, transactionService, auftragsServicesFürTransportplanung, unterbeauftragungServicesFürTransportplanung, transportnetzServicesFürTransportplanung, timeServicesMock.Object);
            IAuftragServicesFuerBuchhaltung auftragServicesFuerBuchhaltung = auftragsServices as IAuftragServicesFuerBuchhaltung;
            buchhaltungServices = new BuchhaltungKomponenteFacade(
                persistenceService, 
                transactionService, 
                unterbeauftragungsServicesFuerBuchhaltung, 
                bankServicesFuerBuchhaltung,
                transportplanServicesFuerBuchhaltung,
                auftragServicesFuerBuchhaltung,
                geschaeftspartnerServices,
                pDFErzeugungsServicesFuerBuchhaltung);
            ITransportplanungServicesFürAuftrag transportplanungServicesFürAuftrag = new TransportplanungKomponenteFacade(persistenceService, transactionService, auftragsServicesFürTransportplanung, unterbeauftragungServicesFürTransportplanung, transportnetzServicesFürTransportplanung, timeServicesMock.Object);
            auftragsServicesFürTransportplanung.RegisterTransportplanungServiceFürAuftrag(transportplanungServicesFürAuftrag);
            transportplanungsServicesFuerSendung = new TransportplanungKomponenteFacade(persistenceService, transactionService, auftragsServicesFürTransportplanung, unterbeauftragungServicesFürTransportplanung, transportnetzServicesFürTransportplanung, timeServicesMock.Object);

            hamburgLokation = new LokationDTO("Hamburg", TimeSpan.Parse("10"), 10);
            bremerhavenLokation = new LokationDTO("Bremerhaven", TimeSpan.Parse("15"), 15);
            shanghaiLokation = new LokationDTO("Shanghai", TimeSpan.Parse("10"), 10);

            transportnetzServices.CreateLokation(ref hamburgLokation);
            transportnetzServices.CreateLokation(ref bremerhavenLokation);
            transportnetzServices.CreateLokation(ref shanghaiLokation);

            hh_bhv = new TransportbeziehungDTO(hamburgLokation, bremerhavenLokation);
            bhv_sh = new TransportbeziehungDTO(bremerhavenLokation, shanghaiLokation);

            transportnetzServices.CreateTransportbeziehung(ref hh_bhv);
            transportnetzServices.CreateTransportbeziehung(ref bhv_sh);

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
            frv_hh_bhv.KapazitaetTEU = 2;
            frv_hh_bhv.KostenFix = 1000;
            frv_hh_bhv.KostenProTEU = 100;
            frv_hh_bhv.KostenProFEU = 200;
            frv_hh_bhv.FuerTransportAufTransportbeziehung = hh_bhv.TbNr;
            frv_hh_bhv.Frachtfuehrer = frfHH_BHV;
            frv_hh_bhv.Zeitvorgabe = TimeSpan.Parse("2"); // 2 Tage
            unterbeauftragungsServices.CreateFrachtfuehrerRahmenvertrag(ref frv_hh_bhv);

            FrachtfuehrerDTO frfBHV = new FrachtfuehrerDTO();
            unterbeauftragungsServices.CreateFrachtfuehrer(ref frfBHV);
            FrachtfuehrerRahmenvertragDTO frv_bhv_sh = new FrachtfuehrerRahmenvertragDTO();
            frv_bhv_sh.GueltigkeitAb = DateTime.Parse("01.01.2013");
            frv_bhv_sh.GueltigkeitBis = DateTime.Parse("31.12.2013");
            frv_bhv_sh.Abfahrtszeiten = new System.Collections.Generic.List<StartzeitDTO>() 
            { 
                new StartzeitDTO() { Wochentag = DayOfWeek.Monday, Uhrzeit = 8 },
                new StartzeitDTO() { Wochentag = DayOfWeek.Thursday, Uhrzeit = 10 },
                new StartzeitDTO() { Wochentag = DayOfWeek.Saturday, Uhrzeit = 8 }
            };
            frv_bhv_sh.KapazitaetTEU = 4;
            frv_bhv_sh.KostenFix = 2000;
            frv_bhv_sh.KostenProTEU = 200;
            frv_bhv_sh.KostenProFEU = 400;
            frv_bhv_sh.FuerTransportAufTransportbeziehung = bhv_sh.TbNr;
            frv_bhv_sh.Frachtfuehrer = frfBHV;
            frv_bhv_sh.Zeitvorgabe = TimeSpan.Parse("5"); // 5 Tage
            unterbeauftragungsServices.CreateFrachtfuehrerRahmenvertrag(ref frv_bhv_sh);

            IList<LokationDTO> lokationen = new List<LokationDTO>();
            lokationen.Add(hh_bhv.Start);
            lokationen.Add(hh_bhv.Ziel);
            lokationen.Add(bhv_sh.Start);
            lokationen.Add(bhv_sh.Ziel);

            ITransportplanungServicesFuerSendung transportplanungServicesFuerSendung = new TransportplanungKomponenteFacade(persistenceService, transactionService, auftragsServicesFürTransportplanung, unterbeauftragungServicesFürTransportplanung, transportnetzServicesFürTransportplanung, timeServicesMock.Object);
            IBuchhaltungServicesFuerSendung buchhaltungServiceFuerSendung = new BuchhaltungKomponenteFacade(
                persistenceService, 
                transactionService, 
                unterbeauftragungsServicesFuerBuchhaltung, 
                bankServicesFuerBuchhaltung,
                transportplanServicesFuerBuchhaltung,
                auftragServicesFuerBuchhaltung,
                geschaeftspartnerServices,
                pDFErzeugungsServicesFuerBuchhaltung);
            SendungKomponenteFacade sendungKomponenteFacade = new SendungKomponenteFacade(
                persistenceService,
                transactionService,
                transportplanungServicesFuerSendung,
                auftragsServices,
                buchhaltungServiceFuerSendung);
            sendungServices = sendungKomponenteFacade;
            ISendungServicesfürLokationsAdapter sendungServicesfürLokationsAdapter = sendungKomponenteFacade;
            lokationAdapter = new LokationsApaterFacade(sendungServicesfürLokationsAdapter, lokationen);
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
                persistenceService.ExecuteSQLQuery("DELETE FROM SENDUNGSPOSITION");
                persistenceService.ExecuteSQLQuery("DELETE FROM SENDUNGSVERFOLGUNGSEREIGNIS");
                persistenceService.ExecuteSQLQuery("DELETE FROM SENDUNG");
            });
        }

        [TestMethod, TestCategory("IntegrationTest")]
        public void TestErstelleSendungUndOrdneSendungsereignisseZuSuccess()
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
            SendungsanfrageDTO saDTO = new SendungsanfrageDTO();
            SendungspositionDTO sp1 = new SendungspositionDTO();
            saDTO.Sendungspositionen.Add(sp1);
            saDTO.AbholzeitfensterStart = DateTime.Parse("01.09.2013");
            saDTO.AbholzeitfensterEnde = DateTime.Parse("10.09.2013");
            saDTO.AngebotGültigBis = DateTime.Now.AddHours(1);
            saDTO.StartLokation = hamburgLokation.LokNr;
            saDTO.ZielLokation = shanghaiLokation.LokNr;
            saDTO.Auftraggeber = 1;

            auftragsServices.CreateSendungsanfrage(ref saDTO);
            auftragsServices.PlaneSendungsanfrage(saDTO.SaNr);
            List<TransportplanDTO> pläne = transportplanungsServices.FindTransportplaeneZuSendungsanfrage(saDTO.SaNr);
            Assert.IsTrue(pläne.Count >= 1);

            TransportplanDTO planÜberBhv = pläne.Find((plan) =>
            {
                return plan.TransportplanSchritte.ToList().Find((tps) =>
                {
                    TransportAktivitaetDTO ta = tps as TransportAktivitaetDTO;
                    if (ta != null)
                    {
                        return ta.FuerTransportAufTransportbeziehung == hh_bhv.TbNr;
                    }
                    else
                    {
                        return false;
                    }
                }) != null;
            });
            Assert.IsTrue(planÜberBhv != null);

            Assert.IsTrue(planÜberBhv.TransportplanSchritte.Count == 2);
            pläne = transportplanungsServices.FindTransportplaeneZuSendungsanfrage(saDTO.SaNr);
            Assert.IsTrue(pläne.Count == 1);
            Assert.IsTrue(pläne[0].TpNr == planÜberBhv.TpNr);

            foreach (TransportplanDTO tpDTO in pläne)
            {
                Sendungsanfrage sa = auftragsServices.FindSendungsanfrage(tpDTO.SaNr).ToEntity();
                auftragsServices.NimmAngebotAn(sa.SaNr);
                sendungServices.ErstelleSendung(tpDTO.TpNr, saDTO.SaNr);
            }

            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            var t = Task.Factory.StartNew(
                () =>
                {
                    lokationAdapter.EmpfangeSendungsverfolgungsereignisAusQueue();
                    token.ThrowIfCancellationRequested();
                }, 
                token);

            Thread.Sleep(10000);
            lokationAdapter.SetEmpfangeSendungsverfolgungsereignis(false);
            Thread.Sleep(5000);
            
            tokenSource.Cancel();
        }
    }
}
