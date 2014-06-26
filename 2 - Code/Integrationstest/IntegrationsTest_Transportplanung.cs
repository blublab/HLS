using ApplicationCore.AuftragKomponente.AccessLayer;
using ApplicationCore.AuftragKomponente.DataAccessLayer;
using ApplicationCore.FrachtfuehrerAdapter.AccessLayer;
using ApplicationCore.TransportnetzKomponente.AccessLayer;
using ApplicationCore.TransportnetzKomponente.DataAccessLayer;
using ApplicationCore.TransportplanungKomponente.AccessLayer;
using ApplicationCore.TransportplanungKomponente.DataAccessLayer;
using ApplicationCore.UnterbeauftragungKomponente.AccessLayer;
using ApplicationCore.UnterbeauftragungKomponente.DataAccessLayer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Util.PersistenceServices.Implementations;
using Util.PersistenceServices.Interfaces;
using Util.TimeServices;

namespace Tests.IntegrationsTest
{
    [TestClass]
    public class IntegrationsTest_Transportplanung
    {
        private static IPersistenceServices persistenceService = null;
        private static ITransactionServices transactionService = null;

        private static ITransportplanungServices transportplanungsServices = null;
        private static ITransportplanungServicesFuerSendung transportplanungServicesFuerSendung = null;
        private static IAuftragServices auftragsServices = null;
        private static IUnterbeauftragungServices unterbeauftragungsServices = null;
        private static ITransportnetzServices transportnetzServices = null;
        private static Mock<IFrachtfuehrerServicesFürUnterbeauftragung> frachtfuehrerServicesMock = null;

        private static LokationDTO hamburgLokation;
        private static LokationDTO bremerhavenLokation;
        private static LokationDTO rotterdamLokation;
        private static LokationDTO shanghaiLokation;
        private static LokationDTO osakaLokation;
        private static TransportbeziehungDTO hh_bhv;
        private static TransportbeziehungDTO bhv_sh;
        private static TransportbeziehungDTO hh_rtd;
        private static TransportbeziehungDTO rtd_sh;
        private static FrachtfuehrerRahmenvertragDTO frv_hh_bhv;
        private static FrachtfuehrerRahmenvertragDTO frv_bhv_sh;
        private static FrachtfuehrerRahmenvertragDTO frv_hh_rtd;
        private static FrachtfuehrerRahmenvertragDTO frv_rtd_sh;

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
            unterbeauftragungsServices = new UnterbeauftragungKomponenteFacade(persistenceService, transactionService, frachtfuehrerServicesMock.Object);
            transportplanungsServices = new TransportplanungKomponenteFacade(persistenceService, transactionService, auftragsServicesFürTransportplanung, unterbeauftragungsServices as IUnterbeauftragungServicesFürTransportplanung, transportnetzServices as ITransportnetzServicesFürTransportplanung, timeServicesMock.Object);
            auftragsServicesFürTransportplanung.RegisterTransportplanungServiceFürAuftrag(transportplanungsServices as ITransportplanungServicesFürAuftrag);
            transportplanungServicesFuerSendung = transportplanungsServices as ITransportplanungServicesFuerSendung;

            hamburgLokation = new LokationDTO("Hamburg", TimeSpan.Parse("10"), 10);
            bremerhavenLokation = new LokationDTO("Bremerhaven", TimeSpan.Parse("15"), 15);
            rotterdamLokation = new LokationDTO("Rotterdam", TimeSpan.Parse("20"), 20);
            shanghaiLokation = new LokationDTO("Shanghai", TimeSpan.Parse("10"), 10);
            osakaLokation = new LokationDTO("Osaka", TimeSpan.Parse("10"), 10);

            transportnetzServices.CreateLokation(ref hamburgLokation);
            transportnetzServices.CreateLokation(ref bremerhavenLokation);
            transportnetzServices.CreateLokation(ref rotterdamLokation);
            transportnetzServices.CreateLokation(ref shanghaiLokation);
            transportnetzServices.CreateLokation(ref osakaLokation);

            hh_bhv = new TransportbeziehungDTO(hamburgLokation, bremerhavenLokation);
            bhv_sh = new TransportbeziehungDTO(bremerhavenLokation, shanghaiLokation);
            hh_rtd = new TransportbeziehungDTO(hamburgLokation, rotterdamLokation);
            rtd_sh = new TransportbeziehungDTO(rotterdamLokation, shanghaiLokation);

            transportnetzServices.CreateTransportbeziehung(ref hh_bhv);
            transportnetzServices.CreateTransportbeziehung(ref bhv_sh);
            transportnetzServices.CreateTransportbeziehung(ref hh_rtd);
            transportnetzServices.CreateTransportbeziehung(ref rtd_sh);

            FrachtfuehrerDTO frfHH_BHV = new FrachtfuehrerDTO();
            unterbeauftragungsServices.CreateFrachtfuehrer(ref frfHH_BHV);
            frv_hh_bhv = new FrachtfuehrerRahmenvertragDTO();
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

            FrachtfuehrerDTO frfBHV_SH = new FrachtfuehrerDTO();
            unterbeauftragungsServices.CreateFrachtfuehrer(ref frfBHV_SH);
            frv_bhv_sh = new FrachtfuehrerRahmenvertragDTO();
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
            frv_bhv_sh.Frachtfuehrer = frfBHV_SH;
            frv_bhv_sh.Zeitvorgabe = TimeSpan.Parse("5"); // 5 Tage
            unterbeauftragungsServices.CreateFrachtfuehrerRahmenvertrag(ref frv_bhv_sh);

            FrachtfuehrerDTO frfHH_RTD = new FrachtfuehrerDTO();
            unterbeauftragungsServices.CreateFrachtfuehrer(ref frfHH_RTD);
            frv_hh_rtd = new FrachtfuehrerRahmenvertragDTO();
            frv_hh_rtd.GueltigkeitAb = DateTime.Parse("01.09.2013");
            frv_hh_rtd.GueltigkeitBis = DateTime.Parse("31.12.2013");
            frv_hh_rtd.Abfahrtszeiten = new System.Collections.Generic.List<StartzeitDTO>() 
            { 
                new StartzeitDTO() { Wochentag = DayOfWeek.Monday, Uhrzeit = 8 },
                new StartzeitDTO() { Wochentag = DayOfWeek.Thursday, Uhrzeit = 10 },
                new StartzeitDTO() { Wochentag = DayOfWeek.Saturday, Uhrzeit = 8 }
            };
            frv_hh_rtd.KapazitaetTEU = 20;
            frv_hh_rtd.KostenFix = 2000;
            frv_hh_rtd.KostenProTEU = 200;
            frv_hh_rtd.KostenProFEU = 400;
            frv_hh_rtd.FuerTransportAufTransportbeziehung = hh_rtd.TbNr;
            frv_hh_rtd.Frachtfuehrer = frfHH_RTD;
            frv_hh_rtd.Zeitvorgabe = TimeSpan.Parse("3"); // 3 Tage
            unterbeauftragungsServices.CreateFrachtfuehrerRahmenvertrag(ref frv_hh_rtd);

            FrachtfuehrerDTO frfRTD_SH = new FrachtfuehrerDTO();
            unterbeauftragungsServices.CreateFrachtfuehrer(ref frfRTD_SH);
            frv_rtd_sh = new FrachtfuehrerRahmenvertragDTO();
            frv_rtd_sh.GueltigkeitAb = DateTime.Parse("01.09.2013");
            frv_rtd_sh.GueltigkeitBis = DateTime.Parse("31.12.2013");
            frv_rtd_sh.Abfahrtszeiten = new System.Collections.Generic.List<StartzeitDTO>() 
            { 
                new StartzeitDTO() { Wochentag = DayOfWeek.Monday, Uhrzeit = 8 },
                new StartzeitDTO() { Wochentag = DayOfWeek.Thursday, Uhrzeit = 10 },
                new StartzeitDTO() { Wochentag = DayOfWeek.Saturday, Uhrzeit = 8 }
            };
            frv_rtd_sh.KapazitaetTEU = 20;
            frv_rtd_sh.KostenFix = 3000;
            frv_rtd_sh.KostenProTEU = 300;
            frv_rtd_sh.KostenProFEU = 400;
            frv_rtd_sh.FuerTransportAufTransportbeziehung = rtd_sh.TbNr;
            frv_rtd_sh.Frachtfuehrer = frfRTD_SH;
            frv_rtd_sh.Zeitvorgabe = TimeSpan.Parse("7"); // 7 Tage
            unterbeauftragungsServices.CreateFrachtfuehrerRahmenvertrag(ref frv_rtd_sh);
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
        public void TestPlanungEinSegmentSuccess()
        {
            SendungsanfrageDTO saDTO = new SendungsanfrageDTO();
            saDTO.Sendungspositionen.Add(new SendungspositionDTO());
            saDTO.AbholzeitfensterStart = DateTime.Parse("29.07.2013");
            saDTO.AbholzeitfensterEnde = DateTime.Parse("04.08.2013");
            saDTO.StartLokation = hamburgLokation.LokNr;
            saDTO.ZielLokation = bremerhavenLokation.LokNr;

            Assert.IsTrue(saDTO.Status == SendungsanfrageStatusTyp.NichtErfasst);
            auftragsServices.CreateSendungsanfrage(ref saDTO);
            Assert.IsTrue(saDTO.Status == SendungsanfrageStatusTyp.Erfasst);
            auftragsServices.PlaneSendungsanfrage(saDTO.SaNr);
            List<TransportplanDTO> pläne = transportplanungsServices.FindTransportplaeneZuSendungsanfrage(saDTO.SaNr);
            Assert.IsTrue(pläne.Count == 1);
            TransportplanDTO plan = pläne.First();
            Assert.IsTrue(plan.TransportplanSchritte.Count == 1);
            Assert.IsTrue(plan.AbholungAm == DateTime.Parse("30.07.2013 08:00:00"));
            Assert.IsTrue(plan.LieferungAm == DateTime.Parse("01.08.2013 08:00:00"));
            Assert.IsTrue(plan.Dauer == TimeSpan.Parse("2"));
            Assert.IsTrue(plan.Frachteinheiten.Count == 1);

            // Fixkosten 1000
            // Wartezeit 32 Stunden * 10 = 320
            // Transport 1 TEU = 100
            Assert.IsTrue(plan.Kosten == 1420);
        }

        [TestMethod, TestCategory("IntegrationTest")]
        public void TestPlanungEinSegmentMitFEUSuccess()
        {
            SendungsanfrageDTO saDTO = new SendungsanfrageDTO();
            //// Erzeugt werden soll 1 FEU
            SendungspositionDTO spDTO = new SendungspositionDTO();
            spDTO.Bruttogewicht = TEU.MAXZULADUNG_TONS + 1; // zu groß für TEU -> FEU soll erzeugt werden
            saDTO.Sendungspositionen.Add(spDTO);
            saDTO.AbholzeitfensterStart = DateTime.Parse("29.07.2013");
            saDTO.AbholzeitfensterEnde = DateTime.Parse("04.08.2013");
            saDTO.StartLokation = hamburgLokation.LokNr;
            saDTO.ZielLokation = bremerhavenLokation.LokNr;

            auftragsServices.CreateSendungsanfrage(ref saDTO);
            auftragsServices.PlaneSendungsanfrage(saDTO.SaNr);
            List<TransportplanDTO> pläne = transportplanungsServices.FindTransportplaeneZuSendungsanfrage(saDTO.SaNr);
            Assert.IsTrue(pläne.Count == 1);
            TransportplanDTO plan = pläne.First();
            Assert.IsTrue(plan.TransportplanSchritte.Count == 1);
            Assert.IsTrue(plan.AbholungAm == DateTime.Parse("30.07.2013 08:00:00"));
            Assert.IsTrue(plan.LieferungAm == DateTime.Parse("01.08.2013 08:00:00"));
            Assert.IsTrue(plan.Dauer == TimeSpan.Parse("2"));
            Assert.IsTrue(plan.Frachteinheiten.Count == 1);
            Assert.IsTrue(plan.Frachteinheiten.Any((fe) => { return fe.FraeTyp == FrachteinheitTyp.FEU; }));

            // Fixkosten 1000
            // Wartezeit 32 Stunden * 10 = 320
            // Transport 1 FEU = 200
            Assert.IsTrue(plan.Kosten == 1520);
        }

        [TestMethod, TestCategory("IntegrationTest")]
        public void TestPlanungEinSegmentMitFEUundTEUSuccess()
        {
            SendungsanfrageDTO saDTO = new SendungsanfrageDTO();
            //// Erzeugt werden sollen 1 FEU und 1 TEU
            SendungspositionDTO spDTO = new SendungspositionDTO();
            spDTO.Bruttogewicht = TEU.MAXZULADUNG_TONS;
            saDTO.Sendungspositionen.Add(spDTO);
            spDTO = new SendungspositionDTO();
            spDTO.Bruttogewicht = TEU.MAXZULADUNG_TONS + 1; // zu groß für TEU -> FEU soll erzeugt werden
            saDTO.Sendungspositionen.Add(spDTO);
            saDTO.AbholzeitfensterStart = DateTime.Parse("29.07.2013");
            saDTO.AbholzeitfensterEnde = DateTime.Parse("04.08.2013");
            saDTO.StartLokation = bremerhavenLokation.LokNr;
            saDTO.ZielLokation = shanghaiLokation.LokNr;

            auftragsServices.CreateSendungsanfrage(ref saDTO);
            auftragsServices.PlaneSendungsanfrage(saDTO.SaNr);
            List<TransportplanDTO> pläne = transportplanungsServices.FindTransportplaeneZuSendungsanfrage(saDTO.SaNr);
            Assert.IsTrue(pläne.Count == 1);
            TransportplanDTO plan = pläne.First();
            Assert.IsTrue(plan.TransportplanSchritte.Count == 1);
            Assert.IsTrue(plan.AbholungAm == DateTime.Parse("29.07.2013 08:00:00"));
            Assert.IsTrue(plan.LieferungAm == DateTime.Parse("03.08.2013 08:00:00"));
            Assert.IsTrue(plan.Dauer == TimeSpan.Parse("5"));
            Assert.IsTrue(plan.Frachteinheiten.Count == 2);
            Assert.IsTrue(plan.Frachteinheiten.Any((fe) => { return fe.FraeTyp == FrachteinheitTyp.TEU; }));
            Assert.IsTrue(plan.Frachteinheiten.Any((fe) => { return fe.FraeTyp == FrachteinheitTyp.FEU; }));

            // Fixkosten 2000
            // Wartezeit 8 Stunden * 15 = 120
            // Transport 1 FEU = 400, 1 TEU 200
            Assert.IsTrue(plan.Kosten == 2720);
        }

        [TestMethod, TestCategory("IntegrationTest")]
        public void TestPlanungEinSegmentFailWegenZuSchwererSendungsposition()
        {
            SendungsanfrageDTO saDTO = new SendungsanfrageDTO();
            SendungspositionDTO spDTO = new SendungspositionDTO();
            spDTO.Bruttogewicht = FEU.MAXZULADUNG_TONS + 1; // Es können auf Grund des Gewichtes keine Frachteinheiten erzeugt werden
            saDTO.Sendungspositionen.Add(spDTO);
            saDTO.AbholzeitfensterStart = DateTime.Parse("29.07.2013");
            saDTO.AbholzeitfensterEnde = DateTime.Parse("04.08.2013");
            saDTO.StartLokation = hamburgLokation.LokNr;
            saDTO.ZielLokation = bremerhavenLokation.LokNr;

            auftragsServices.CreateSendungsanfrage(ref saDTO);
            List<TransportplanungMeldung> meldungen = auftragsServices.PlaneSendungsanfrage(saDTO.SaNr);
            Assert.IsTrue(meldungen.Count == 1);
            Assert.IsTrue(meldungen[0].Tag == TransportplanungMeldungTag.FrachteinheitenBildungNichtMöglich);
        }

        [TestMethod, TestCategory("IntegrationTest")]
        public void TestPlanungEinSegmentZweiTEUSuccess()
        {
            SendungsanfrageDTO saDTO = new SendungsanfrageDTO();
            SendungspositionDTO sp1 = new SendungspositionDTO();
            sp1.Bruttogewicht = TEU.MAXZULADUNG_TONS;
            saDTO.Sendungspositionen.Add(sp1);
            SendungspositionDTO sp2 = new SendungspositionDTO();
            sp2.Bruttogewicht = TEU.MAXZULADUNG_TONS;
            saDTO.Sendungspositionen.Add(sp2);
            saDTO.AbholzeitfensterStart = DateTime.Parse("29.07.2013");
            saDTO.AbholzeitfensterEnde = DateTime.Parse("04.08.2013");
            saDTO.StartLokation = hamburgLokation.LokNr;
            saDTO.ZielLokation = bremerhavenLokation.LokNr;

            auftragsServices.CreateSendungsanfrage(ref saDTO);
            auftragsServices.PlaneSendungsanfrage(saDTO.SaNr);
            List<TransportplanDTO> pläne = transportplanungsServices.FindTransportplaeneZuSendungsanfrage(saDTO.SaNr);
            Assert.IsTrue(pläne.Count == 1);
            TransportplanDTO plan = pläne.First();
            Assert.IsTrue(plan.TransportplanSchritte.Count == 1);
            Assert.IsTrue(plan.AbholungAm == DateTime.Parse("30.07.2013 08:00:00"));
            Assert.IsTrue(plan.LieferungAm == DateTime.Parse("01.08.2013 08:00:00"));
            Assert.IsTrue(plan.Dauer == TimeSpan.Parse("2"));

            // Fixkosten 1000
            // Wartezeit 32 Stunden * 10 = 320
            // Transport 2 TEU = 200
            Assert.IsTrue(plan.Kosten == 1520);
        }

        [TestMethod, TestCategory("IntegrationTest")]
        public void TestPlanungEinSegmentDreiTEUFailWegenZuGeringerKapazität()
        {
            SendungsanfrageDTO saDTO = new SendungsanfrageDTO();
            SendungspositionDTO sp1 = new SendungspositionDTO();
            sp1.Bruttogewicht = TEU.MAXZULADUNG_TONS;
            saDTO.Sendungspositionen.Add(sp1);
            SendungspositionDTO sp2 = new SendungspositionDTO();
            sp2.Bruttogewicht = TEU.MAXZULADUNG_TONS;
            saDTO.Sendungspositionen.Add(sp2);
            SendungspositionDTO sp3 = new SendungspositionDTO();
            sp3.Bruttogewicht = TEU.MAXZULADUNG_TONS;
            saDTO.Sendungspositionen.Add(sp3);
            saDTO.AbholzeitfensterStart = DateTime.Parse("29.07.2013");
            saDTO.AbholzeitfensterEnde = DateTime.Parse("04.08.2013");
            saDTO.StartLokation = hamburgLokation.LokNr;
            saDTO.ZielLokation = bremerhavenLokation.LokNr;

            auftragsServices.CreateSendungsanfrage(ref saDTO);
            auftragsServices.PlaneSendungsanfrage(saDTO.SaNr);
            List<TransportplanDTO> pläne2 = transportplanungsServices.FindTransportplaeneZuSendungsanfrage(saDTO.SaNr);
            Assert.IsTrue(pläne2.Count == 0); // Kapazität des FRV nur zwei TEU
        }

        [TestMethod, TestCategory("IntegrationTest")]
        public void TestPlanungEinSegmentZweiSendungsanfragenSuccessDieDritteFailWegenKeineKapazität()
        {
            SendungsanfrageDTO saDTO1 = new SendungsanfrageDTO();
            saDTO1.Sendungspositionen.Add(new SendungspositionDTO());
            saDTO1.AbholzeitfensterStart = DateTime.Parse("29.07.2013");
            saDTO1.AbholzeitfensterEnde = DateTime.Parse("04.08.2013");
            saDTO1.StartLokation = hamburgLokation.LokNr;
            saDTO1.ZielLokation = bremerhavenLokation.LokNr;

            auftragsServices.CreateSendungsanfrage(ref saDTO1);
            auftragsServices.PlaneSendungsanfrage(saDTO1.SaNr);
            List<TransportplanDTO> pläne1 = transportplanungsServices.FindTransportplaeneZuSendungsanfrage(saDTO1.SaNr);
            Assert.IsTrue(pläne1.Count == 1); // Rest durch TestPlanungEinSegmentSuccess geprüft

            SendungsanfrageDTO saDTO2 = new SendungsanfrageDTO();
            saDTO2.Sendungspositionen.Add(new SendungspositionDTO());
            saDTO2.AbholzeitfensterStart = DateTime.Parse("30.07.2013");
            saDTO2.AbholzeitfensterEnde = DateTime.Parse("04.08.2013");
            saDTO2.StartLokation = hamburgLokation.LokNr;
            saDTO2.ZielLokation = bremerhavenLokation.LokNr;

            auftragsServices.CreateSendungsanfrage(ref saDTO2);
            auftragsServices.PlaneSendungsanfrage(saDTO2.SaNr);
            List<TransportplanDTO> pläne2 = transportplanungsServices.FindTransportplaeneZuSendungsanfrage(saDTO2.SaNr);
            Assert.IsTrue(pläne2.Count == 1);
            TransportplanDTO plan2 = pläne2.First();
            Assert.IsTrue(plan2.TransportplanSchritte.Count == 1);
            Assert.IsTrue(plan2.AbholungAm == DateTime.Parse("30.07.2013 08:00:00"));
            Assert.IsTrue(plan2.LieferungAm == DateTime.Parse("01.08.2013 08:00:00"));
            Assert.IsTrue(plan2.Dauer == TimeSpan.Parse("2"));

            // Fixkosten 1000
            // Wartezeit 8 Stunden * 10 = 80
            // Transport 1 TEU = 100
            Assert.IsTrue(plan2.Kosten == 1180);

            SendungsanfrageDTO saDTO3 = new SendungsanfrageDTO();
            saDTO3.Sendungspositionen.Add(new SendungspositionDTO());
            saDTO3.AbholzeitfensterStart = DateTime.Parse("29.07.2013");
            saDTO3.AbholzeitfensterEnde = DateTime.Parse("30.07.2013");
            saDTO3.StartLokation = hamburgLokation.LokNr;
            saDTO3.ZielLokation = bremerhavenLokation.LokNr;

            auftragsServices.CreateSendungsanfrage(ref saDTO3);
            auftragsServices.PlaneSendungsanfrage(saDTO3.SaNr);
            List<TransportplanDTO> pläne3 = transportplanungsServices.FindTransportplaeneZuSendungsanfrage(saDTO3.SaNr);
            Assert.IsTrue(pläne3.Count == 0); // keine weitere Kapazität für dieses Zeitfenster
        }

        [TestMethod, TestCategory("IntegrationTest")]
        public void TestPlanungEinSegmentFailWegenAbholzeitfensterAußerhalbAbfahrtszeiten()
        {
            SendungsanfrageDTO saDTO = new SendungsanfrageDTO();
            saDTO.Sendungspositionen.Add(new SendungspositionDTO());
            saDTO.AbholzeitfensterStart = DateTime.Parse("27.07.2013");
            saDTO.AbholzeitfensterEnde = DateTime.Parse("29.07.2013");
            saDTO.StartLokation = hamburgLokation.LokNr;
            saDTO.ZielLokation = bremerhavenLokation.LokNr;

            auftragsServices.CreateSendungsanfrage(ref saDTO);
            auftragsServices.PlaneSendungsanfrage(saDTO.SaNr);
            List<TransportplanDTO> pläne = transportplanungsServices.FindTransportplaeneZuSendungsanfrage(saDTO.SaNr);
            Assert.IsTrue(pläne.Count == 0);
        }

        [TestMethod, TestCategory("IntegrationTest")]
        public void TestPlanungZweiSegmenteSuccess()
        {
            SendungsanfrageDTO saDTO = new SendungsanfrageDTO();
            SendungspositionDTO sp1 = new SendungspositionDTO();
            saDTO.Sendungspositionen.Add(sp1);
            saDTO.AbholzeitfensterStart = DateTime.Parse("29.07.2013");
            saDTO.AbholzeitfensterEnde = DateTime.Parse("04.08.2013");
            saDTO.StartLokation = hamburgLokation.LokNr;
            saDTO.ZielLokation = shanghaiLokation.LokNr;

            auftragsServices.CreateSendungsanfrage(ref saDTO);
            auftragsServices.PlaneSendungsanfrage(saDTO.SaNr);
            List<TransportplanDTO> pläne = transportplanungsServices.FindTransportplaeneZuSendungsanfrage(saDTO.SaNr);
            Assert.IsTrue(pläne.Count == 1);
            TransportplanDTO plan = pläne.First();
            Assert.IsTrue(plan.TransportplanSchritte.Count == 2);
            Assert.IsTrue(plan.AbholungAm == DateTime.Parse("30.07.2013 08:00:00"));
            Assert.IsTrue(plan.LieferungAm == DateTime.Parse("06.08.2013 10:00:00"));
            Assert.IsTrue(plan.Dauer == TimeSpan.Parse("7.02:00"));

            // HH: 32 Stunden * 10
            // HH->BHV: 1000 + 100
            // BHV: 2 Stunden * 15
            // BHV->SH: 2000 + 200
            Assert.IsTrue(plan.Kosten == 3650);
        }

        [TestMethod, TestCategory("IntegrationTest")]
        public void TestPlanungZweiWegeSuccess()
        {
            SendungsanfrageDTO saDTO = new SendungsanfrageDTO();
            SendungspositionDTO sp1 = new SendungspositionDTO();
            saDTO.Sendungspositionen.Add(sp1);
            saDTO.AbholzeitfensterStart = DateTime.Parse("01.09.2013");
            saDTO.AbholzeitfensterEnde = DateTime.Parse("10.09.2013");
            saDTO.StartLokation = hamburgLokation.LokNr;
            saDTO.ZielLokation = shanghaiLokation.LokNr;

            auftragsServices.CreateSendungsanfrage(ref saDTO);
            auftragsServices.PlaneSendungsanfrage(saDTO.SaNr);
            List<TransportplanDTO> pläne = transportplanungsServices.FindTransportplaeneZuSendungsanfrage(saDTO.SaNr);
            Assert.IsTrue(pläne.Count == 2);
            TransportplanDTO plan1 = pläne.Find((plan) =>
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
            Assert.IsTrue(plan1 != null);
            Assert.IsTrue(plan1.TransportplanSchritte.Count == 2);
            Assert.IsTrue(plan1.AbholungAm == DateTime.Parse("03.09.2013 08:00:00"));
            Assert.IsTrue(plan1.LieferungAm == DateTime.Parse("10.09.2013 10:00:00"));
            Assert.IsTrue(plan1.Dauer == TimeSpan.Parse("7.02:00"));
            Assert.IsTrue(plan1.Frachteinheiten.Count == 1);

            // HH: 56 Stunden * 10
            // HH->BHV: 1000 + 100
            // BHV: 2 Stunden * 15
            // BHV->SH: 2000 + 200
            Assert.IsTrue(plan1.Kosten == 3890);

            TransportplanDTO plan2 = pläne.Find((plan) =>
            {
                return plan.TransportplanSchritte.ToList().Find((tps) =>
                {
                    TransportAktivitaetDTO ta = tps as TransportAktivitaetDTO;
                    if (ta != null)
                    {
                        return ta.FuerTransportAufTransportbeziehung == hh_rtd.TbNr;
                    }
                    else
                    {
                        return false;
                    }
                }) != null;
            });
            Assert.IsTrue(plan2 != null);
            Assert.IsTrue(plan2.TransportplanSchritte.Count == 2);
            Assert.IsTrue(plan2.AbholungAm == DateTime.Parse("02.09.2013 08:00:00"));
            Assert.IsTrue(plan2.LieferungAm == DateTime.Parse("12.09.2013 10:00:00"));
            Assert.IsTrue(plan2.Dauer == TimeSpan.Parse("10.02:00"));
            Assert.IsTrue(plan2.Frachteinheiten.Count == 1);

            // HH: 32 Stunden * 10
            // HH->RTD: 2000 + 200
            // RTD: 2 Stunden * 20
            // RTD->SH: 3000 + 300
            Assert.IsTrue(plan2.Kosten == 5860);
        }

        [TestMethod, TestCategory("IntegrationTest")]
        public void TestPlanungZweiWegeWähleTransportplanSuccess()
        {
            SendungsanfrageDTO saDTO = new SendungsanfrageDTO();
            SendungspositionDTO sp1 = new SendungspositionDTO();
            saDTO.Sendungspositionen.Add(sp1);
            saDTO.AbholzeitfensterStart = DateTime.Parse("01.09.2013");
            saDTO.AbholzeitfensterEnde = DateTime.Parse("10.09.2013");
            saDTO.AngebotGültigBis = DateTime.Now.AddHours(1);
            saDTO.StartLokation = hamburgLokation.LokNr;
            saDTO.ZielLokation = shanghaiLokation.LokNr;

            frachtfuehrerServicesMock.Setup(ffs => ffs.SendeFrachtauftragAnFrachtfuehrer(
                It.Is<FrachtauftragDTO>((fraDTO) => fraDTO.FrachtfuehrerRahmenvertrag.FrvNr == frv_hh_bhv.FrvNr)))
                .Callback<FrachtauftragDTO>((fraDTO)
                =>
                {
                    bool ok = true;
                    ok &= fraDTO.PlanStartzeit == DateTime.Parse("03.09.2013 8:00");
                    ok &= fraDTO.PlanEndezeit == DateTime.Parse("05.09.2013 8:00");
                    ok &= fraDTO.VerwendeteKapazitaetTEU == 1;
                    ok &= fraDTO.VerwendeteKapazitaetFEU == 0;
                    if (!ok)
                    {
                        throw new ArgumentException();
                    }
                })
                .Verifiable();
            frachtfuehrerServicesMock.Setup(ffs => ffs.SendeFrachtauftragAnFrachtfuehrer(
                It.Is<FrachtauftragDTO>((fraDTO) => fraDTO.FrachtfuehrerRahmenvertrag.FrvNr == frv_bhv_sh.FrvNr)))
                 .Callback<FrachtauftragDTO>((fraDTO)
                =>
                 {
                     bool ok = true;
                     ok &= fraDTO.PlanStartzeit == DateTime.Parse("05.09.2013 10:00");
                     ok &= fraDTO.PlanEndezeit == DateTime.Parse("10.09.2013 10:00");
                     ok &= fraDTO.VerwendeteKapazitaetTEU == 1;
                     ok &= fraDTO.VerwendeteKapazitaetFEU == 0;
                     if (!ok)
                     {
                         throw new ArgumentException();
                     }
                 })
                 .Verifiable();  

            auftragsServices.CreateSendungsanfrage(ref saDTO);
            auftragsServices.PlaneSendungsanfrage(saDTO.SaNr);
            List<TransportplanDTO> pläne = transportplanungsServices.FindTransportplaeneZuSendungsanfrage(saDTO.SaNr);
            Assert.IsTrue(pläne.Count == 2);
            //// Inhalte geprüft durch TestPlanungZweiWegeSuccess()

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
            auftragsServices.NimmAngebotAn(saDTO.SaNr);
            transportplanungServicesFuerSendung.FühreTransportplanAus(planÜberBhv.TpNr);
            pläne = transportplanungsServices.FindTransportplaeneZuSendungsanfrage(saDTO.SaNr);
            Assert.IsTrue(pläne.Count == 1);
            Assert.IsTrue(pläne[0].TpNr == planÜberBhv.TpNr);
            frachtfuehrerServicesMock.Verify();
        }

        [TestMethod, TestCategory("IntegrationTest")]
        public void TestPlanungFailWegenKeinTransportWeg()
        {
            SendungsanfrageDTO saDTO = new SendungsanfrageDTO();
            saDTO.Sendungspositionen.Add(new SendungspositionDTO());
            saDTO.AbholzeitfensterStart = DateTime.Parse("27.07.2013");
            saDTO.AbholzeitfensterEnde = DateTime.Parse("29.07.2013");
            saDTO.StartLokation = hamburgLokation.LokNr;
            saDTO.ZielLokation = osakaLokation.LokNr;

            auftragsServices.CreateSendungsanfrage(ref saDTO);
            List<TransportplanungMeldung> meldungen = auftragsServices.PlaneSendungsanfrage(saDTO.SaNr);
            Assert.IsTrue(meldungen.Count == 1);
            Assert.IsTrue(meldungen[0].Tag == TransportplanungMeldungTag.KeinWegVorhanden);
        }

        [TestMethod, TestCategory("IntegrationsTest")]
        public void TestPlanungFailWegenAbholzeitfensterÜberschritten()
        {
            SendungsanfrageDTO saDTO = new SendungsanfrageDTO();
            saDTO.Sendungspositionen.Add(new SendungspositionDTO());
            saDTO.AbholzeitfensterStart = DateTime.Parse("29.07.2013");
            saDTO.AbholzeitfensterEnde = DateTime.Parse("04.08.2013");
            saDTO.StartLokation = hamburgLokation.LokNr;
            saDTO.ZielLokation = bremerhavenLokation.LokNr;

            auftragsServices.CreateSendungsanfrage(ref saDTO);
            auftragsServices.PlaneSendungsanfrage(saDTO.SaNr);
            auftragsServices.NimmAngebotAn(saDTO.SaNr);
            List<TransportplanDTO> pläne = transportplanungsServices.FindTransportplaeneZuSendungsanfrage(saDTO.SaNr);
            Assert.IsTrue(pläne.Count == 1);
            transportplanungServicesFuerSendung.FühreTransportplanAus(pläne[0].TpNr);
            pläne = transportplanungsServices.FindTransportplaeneZuSendungsanfrage(saDTO.SaNr);
            Assert.IsTrue(pläne.Count == 1);
            Assert.IsTrue(pläne[0].Meldungen.Count == 1);
            Assert.IsTrue(pläne[0].Meldungen[0].Tag == TransportplanMeldungTyp.MeldungTag.AbholzeitUeberschritten);
            saDTO = auftragsServices.FindSendungsanfrage(saDTO.SaNr);
            Assert.IsTrue(saDTO.Status == SendungsanfrageStatusTyp.Unterbrochen);
        }
    }
}
