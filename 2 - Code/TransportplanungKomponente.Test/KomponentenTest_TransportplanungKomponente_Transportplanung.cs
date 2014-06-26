using ApplicationCore.AuftragKomponente.AccessLayer;
using ApplicationCore.AuftragKomponente.DataAccessLayer;
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

namespace Tests.KomponentenTest.TransportplanungKomponente
{
    [TestClass]
    public class KomponentenTest_TransportplanungKomponente_Transportplanung
    {
        private static IPersistenceServices persistenceService = null;
        private static ITransactionServices transactionService = null;

        private static ITransportplanungServices transportplanungsServices = null;
        private static ITransportplanungServicesFuerSendung transportplanungsServicesFuerSendung = null;
        private static ITransportplanungServicesFürAuftrag transportplanungsServicesFürAuftrag = null;

        private static Lokation hamburgLokation;
        private static Lokation bremerhavenLokation;
        private static Lokation rotterdamLokation;
        private static Lokation shanghaiLokation;
        private static Transportbeziehung hh_bhv;
        private static Transportbeziehung bhv_sh;
        private static Transportbeziehung hh_rtd;
        private static Transportbeziehung rtd_sh;

        private static Mock<IAuftragServicesFürTransportplanung> auftragsServicesFürTransportplanungMock;

        [ClassInitialize]
        public static void InitializeTests(TestContext testContext)
        {
            log4net.Config.XmlConfigurator.Configure();

            PersistenceServicesFactory.CreateSimpleMySQLPersistenceService(out persistenceService, out transactionService);

            auftragsServicesFürTransportplanungMock = new Mock<IAuftragServicesFürTransportplanung>();

            var transportnetzServicesFürTransportplanungMock = new Mock<ITransportnetzServicesFürTransportplanung>();

            hamburgLokation = new Lokation("Hamburg", TimeSpan.Parse("10"), 10);
            hamburgLokation.LokNr = 1;
            bremerhavenLokation = new Lokation("Bremerhaven", TimeSpan.Parse("15"), 15);
            bremerhavenLokation.LokNr = 2;
            rotterdamLokation = new Lokation("Rotterdam", TimeSpan.Parse("20"), 20);
            rotterdamLokation.LokNr = 3;
            shanghaiLokation = new Lokation("Shanghai", TimeSpan.Parse("10"), 10);
            shanghaiLokation.LokNr = 4;

            hh_bhv = new Transportbeziehung(hamburgLokation, bremerhavenLokation);
            hh_bhv.TbNr = 1;
            bhv_sh = new Transportbeziehung(bremerhavenLokation, shanghaiLokation);
            bhv_sh.TbNr = 2;
            hh_rtd = new Transportbeziehung(hamburgLokation, rotterdamLokation);
            hh_rtd.TbNr = 3;
            rtd_sh = new Transportbeziehung(rotterdamLokation, shanghaiLokation);
            rtd_sh.TbNr = 4;

            List<List<Transportbeziehung>> path_hh_bhv = new List<List<Transportbeziehung>>();
            path_hh_bhv.Add(new List<Transportbeziehung>() { hh_bhv });
            transportnetzServicesFürTransportplanungMock.Setup(tns => tns.GeneriereAllePfadeVonBis(hamburgLokation.LokNr, bremerhavenLokation.LokNr))
                .Returns(path_hh_bhv);
            List<List<Transportbeziehung>> path_hh_sh = new List<List<Transportbeziehung>>();
            path_hh_sh.Add(new List<Transportbeziehung>() { hh_bhv, bhv_sh });
            path_hh_sh.Add(new List<Transportbeziehung>() { hh_rtd, rtd_sh });
            transportnetzServicesFürTransportplanungMock.Setup(tns => tns.GeneriereAllePfadeVonBis(hamburgLokation.LokNr, shanghaiLokation.LokNr))
                .Returns(path_hh_sh);

            var unterbeauftragungsServicesMock = new Mock<IUnterbeauftragungServicesFürTransportplanung>();

            // Transportbeauftragung ist im Komponententest unwesentlich
            unterbeauftragungsServicesMock.Setup(ubs => ubs.BeauftrageTransport(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
               .Returns(1);

            FrachtfuehrerRahmenvertrag frv_hh_bhv = new FrachtfuehrerRahmenvertrag();
            frv_hh_bhv.GueltigkeitAb = DateTime.Parse("01.01.2013");
            frv_hh_bhv.GueltigkeitBis = DateTime.Parse("31.12.2013");
            frv_hh_bhv.Abfahrtszeiten = new System.Collections.Generic.List<Startzeit>() 
            { 
                new Startzeit() { Wochentag = DayOfWeek.Tuesday, Uhrzeit = 8 },
                new Startzeit() { Wochentag = DayOfWeek.Wednesday, Uhrzeit = 8 },
                new Startzeit() { Wochentag = DayOfWeek.Friday, Uhrzeit = 8 }
            };
            frv_hh_bhv.KapazitaetTEU = 2;
            frv_hh_bhv.KostenFix = 1000;
            frv_hh_bhv.KostenProTEU = 100;
            frv_hh_bhv.KostenProFEU = 200;
            frv_hh_bhv.FuerTransportAufTransportbeziehung = hh_bhv.TbNr;
            frv_hh_bhv.Zeitvorgabe = TimeSpan.Parse("2"); // 2 Tage
            frv_hh_bhv.FrvNr = 1;

            FrachtfuehrerRahmenvertrag frv_bhv_sh = new FrachtfuehrerRahmenvertrag();
            frv_bhv_sh.GueltigkeitAb = DateTime.Parse("01.01.2013");
            frv_bhv_sh.GueltigkeitBis = DateTime.Parse("31.12.2013");
            frv_bhv_sh.Abfahrtszeiten = new System.Collections.Generic.List<Startzeit>() 
            { 
                new Startzeit() { Wochentag = DayOfWeek.Monday, Uhrzeit = 8 },
                new Startzeit() { Wochentag = DayOfWeek.Thursday, Uhrzeit = 10 },
                new Startzeit() { Wochentag = DayOfWeek.Saturday, Uhrzeit = 8 }
            };
            frv_bhv_sh.KapazitaetTEU = 20;
            frv_bhv_sh.KostenFix = 2000;
            frv_bhv_sh.KostenProTEU = 200;
            frv_bhv_sh.KostenProFEU = 400;
            frv_bhv_sh.FuerTransportAufTransportbeziehung = bhv_sh.TbNr;
            frv_bhv_sh.Zeitvorgabe = TimeSpan.Parse("5"); // 5 Tage
            frv_bhv_sh.FrvNr = 2;

            FrachtfuehrerRahmenvertrag frv_hh_rtd = new FrachtfuehrerRahmenvertrag();
            frv_hh_rtd.GueltigkeitAb = DateTime.Parse("01.09.2013");
            frv_hh_rtd.GueltigkeitBis = DateTime.Parse("31.12.2013");
            frv_hh_rtd.Abfahrtszeiten = new System.Collections.Generic.List<Startzeit>() 
            { 
                new Startzeit() { Wochentag = DayOfWeek.Monday, Uhrzeit = 8 },
                new Startzeit() { Wochentag = DayOfWeek.Thursday, Uhrzeit = 10 },
                new Startzeit() { Wochentag = DayOfWeek.Saturday, Uhrzeit = 8 }
            };
            frv_hh_rtd.KapazitaetTEU = 20;
            frv_hh_rtd.KostenFix = 2000;
            frv_hh_rtd.KostenProTEU = 200;
            frv_hh_rtd.KostenProFEU = 400;
            frv_hh_rtd.FuerTransportAufTransportbeziehung = hh_rtd.TbNr;
            frv_hh_rtd.Zeitvorgabe = TimeSpan.Parse("3"); // 3 Tage
            frv_hh_rtd.FrvNr = 3;

            FrachtfuehrerRahmenvertrag frv_rtd_sh = new FrachtfuehrerRahmenvertrag();
            frv_rtd_sh.GueltigkeitAb = DateTime.Parse("01.09.2013");
            frv_rtd_sh.GueltigkeitBis = DateTime.Parse("31.12.2013");
            frv_rtd_sh.Abfahrtszeiten = new System.Collections.Generic.List<Startzeit>() 
            { 
                new Startzeit() { Wochentag = DayOfWeek.Monday, Uhrzeit = 8 },
                new Startzeit() { Wochentag = DayOfWeek.Thursday, Uhrzeit = 10 },
                new Startzeit() { Wochentag = DayOfWeek.Saturday, Uhrzeit = 8 }
            };
            frv_rtd_sh.KapazitaetTEU = 20;
            frv_rtd_sh.KostenFix = 3000;
            frv_rtd_sh.KostenProTEU = 300;
            frv_rtd_sh.KostenProFEU = 400;
            frv_rtd_sh.FuerTransportAufTransportbeziehung = rtd_sh.TbNr;
            frv_rtd_sh.Zeitvorgabe = TimeSpan.Parse("7"); // 7 Tage
            frv_rtd_sh.FrvNr = 4;

            unterbeauftragungsServicesMock.Setup(ubs => ubs.FindGültigFür(hh_bhv.TbNr, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns((long _, DateTime zeitspanneVon, DateTime zeitspanneBis)
                 =>
                 {
                     if ((zeitspanneVon >= frv_hh_bhv.GueltigkeitAb && zeitspanneVon <= frv_hh_bhv.GueltigkeitBis) ||
                         (zeitspanneBis >= frv_hh_bhv.GueltigkeitAb && zeitspanneBis <= frv_hh_bhv.GueltigkeitBis) ||
                         (zeitspanneVon <= frv_hh_bhv.GueltigkeitAb && zeitspanneBis >= frv_hh_bhv.GueltigkeitBis))
                     {
                         List<FrachtfuehrerRahmenvertrag> lfrv = new List<FrachtfuehrerRahmenvertrag>();
                         lfrv.Add(frv_hh_bhv);
                         return lfrv;
                     }
                     else
                     {
                         return new List<FrachtfuehrerRahmenvertrag>();
                     }
                 });

            unterbeauftragungsServicesMock.Setup(ubs => ubs.FindGültigFür(bhv_sh.TbNr, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns((long _, DateTime zeitspanneVon, DateTime zeitspanneBis)
                 =>
                {
                    if ((zeitspanneVon >= frv_bhv_sh.GueltigkeitAb && zeitspanneVon <= frv_bhv_sh.GueltigkeitBis) ||
                        (zeitspanneBis >= frv_bhv_sh.GueltigkeitAb && zeitspanneBis <= frv_bhv_sh.GueltigkeitBis) ||
                        (zeitspanneVon <= frv_bhv_sh.GueltigkeitAb && zeitspanneBis >= frv_bhv_sh.GueltigkeitBis))
                    {
                        List<FrachtfuehrerRahmenvertrag> lfrv = new List<FrachtfuehrerRahmenvertrag>();
                        lfrv.Add(frv_bhv_sh);
                        return lfrv;
                    }
                    else
                    {
                        return new List<FrachtfuehrerRahmenvertrag>();
                    }
                });

            unterbeauftragungsServicesMock.Setup(ubs => ubs.FindGültigFür(hh_rtd.TbNr, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns((long _, DateTime zeitspanneVon, DateTime zeitspanneBis)
                 =>
                {
                    if ((zeitspanneVon >= frv_hh_rtd.GueltigkeitAb && zeitspanneVon <= frv_hh_rtd.GueltigkeitBis) ||
                        (zeitspanneBis >= frv_hh_rtd.GueltigkeitAb && zeitspanneBis <= frv_hh_rtd.GueltigkeitBis) ||
                        (zeitspanneVon <= frv_hh_rtd.GueltigkeitAb && zeitspanneBis >= frv_hh_rtd.GueltigkeitBis))
                    {
                        List<FrachtfuehrerRahmenvertrag> lfrv = new List<FrachtfuehrerRahmenvertrag>();
                        lfrv.Add(frv_hh_rtd);
                        return lfrv;
                    }
                    else
                    {
                        return new List<FrachtfuehrerRahmenvertrag>();
                    }
                });

            unterbeauftragungsServicesMock.Setup(ubs => ubs.FindGültigFür(rtd_sh.TbNr, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns((long _, DateTime zeitspanneVon, DateTime zeitspanneBis)
                 =>
                {
                    if ((zeitspanneVon >= frv_rtd_sh.GueltigkeitAb && zeitspanneVon <= frv_rtd_sh.GueltigkeitBis) ||
                        (zeitspanneBis >= frv_rtd_sh.GueltigkeitAb && zeitspanneBis <= frv_rtd_sh.GueltigkeitBis) ||
                        (zeitspanneVon <= frv_rtd_sh.GueltigkeitAb && zeitspanneBis >= frv_rtd_sh.GueltigkeitBis))
                    {
                        List<FrachtfuehrerRahmenvertrag> lfrv = new List<FrachtfuehrerRahmenvertrag>();
                        lfrv.Add(frv_rtd_sh);
                        return lfrv;
                    }
                    else
                    {
                        return new List<FrachtfuehrerRahmenvertrag>();
                    }
                });

            var timeServicesMock = new Mock<ITimeServices>();
            //// Wir müssen einen fixen Zeitpunkt simulieren, ansonsten sind bei der Ausführung/Planung evtl. die Verträge oder Angebote abgelaufen
            timeServicesMock.Setup(ts => ts.Now)
               .Returns(DateTime.Parse("31.08.2013 12:00"));

            transportplanungsServices = new TransportplanungKomponenteFacade(persistenceService, transactionService, auftragsServicesFürTransportplanungMock.Object, unterbeauftragungsServicesMock.Object as IUnterbeauftragungServicesFürTransportplanung, transportnetzServicesFürTransportplanungMock.Object as ITransportnetzServicesFürTransportplanung, timeServicesMock.Object);
            transportplanungsServicesFuerSendung = transportplanungsServices as ITransportplanungServicesFuerSendung;
            transportplanungsServicesFürAuftrag = transportplanungsServices as ITransportplanungServicesFürAuftrag;
            Assert.IsTrue(transportplanungsServicesFürAuftrag != null);
        }

        [ClassCleanup]
        public static void CleanUpClass()
        {
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
            });
        }

        [TestMethod]
        public void TestPlanungEinSegmentSuccess()
        {
            SendungsanfrageDTO saDTO = new SendungsanfrageDTO();
            saDTO.SaNr = 1;
            saDTO.Sendungspositionen.Add(new SendungspositionDTO());
            saDTO.AbholzeitfensterStart = DateTime.Parse("29.07.2013");
            saDTO.AbholzeitfensterEnde = DateTime.Parse("04.08.2013");
            saDTO.StartLokation = hamburgLokation.LokNr;
            saDTO.ZielLokation = bremerhavenLokation.LokNr;

            auftragsServicesFürTransportplanungMock.Setup(aus => aus.FindSendungsanfrageEntity(saDTO.SaNr))
                .Returns(saDTO.ToEntity());

            ITransportplanungJob job = transportplanungsServicesFürAuftrag.StarteTransportplanungAsync(saDTO.SaNr);
            job.Wait();
            List<TransportplanDTO> pläne = transportplanungsServices.FindTransportplaeneZuSendungsanfrage(saDTO.SaNr);
            Assert.IsTrue(pläne.Count == 1);
            TransportplanDTO plan = pläne.First();
            Assert.IsTrue(plan.TransportplanSchritte.Count == 1);
            Assert.IsTrue(plan.AbholungAm == DateTime.Parse("30.07.2013 08:00:00"));
            Assert.IsTrue(plan.LieferungAm == DateTime.Parse("01.08.2013 08:00:00"));
            Assert.IsTrue(plan.Dauer == TimeSpan.Parse("2"));

            // Fixkosten 1000
            // Wartezeit 32 Stunden * 10 = 320
            // Transport 1 TEU = 100
            Assert.IsTrue(plan.Kosten == 1420);
        }

        [TestMethod]
        public void TestPlanungEinSegmentMitFEUSuccess()
        {
            SendungsanfrageDTO saDTO = new SendungsanfrageDTO();
            saDTO.SaNr = 1;
            //// Erzeugt werden soll 1 FEU
            SendungspositionDTO sp = new SendungspositionDTO();
            sp.Bruttogewicht = TEU.MAXZULADUNG_TONS + 1; // zu groß für TEU -> FEU soll erzeugt werden
            saDTO.Sendungspositionen.Add(sp);
            saDTO.AbholzeitfensterStart = DateTime.Parse("29.07.2013");
            saDTO.AbholzeitfensterEnde = DateTime.Parse("04.08.2013");
            saDTO.StartLokation = hamburgLokation.LokNr;
            saDTO.ZielLokation = bremerhavenLokation.LokNr;

            auftragsServicesFürTransportplanungMock.Setup(aus => aus.FindSendungsanfrageEntity(saDTO.SaNr))
                .Returns(saDTO.ToEntity());

            ITransportplanungJob job = transportplanungsServicesFürAuftrag.StarteTransportplanungAsync(saDTO.SaNr);
            job.Wait();
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

        [TestMethod]
        public void TestPlanungEinSegmentFailWegenZuSchwererSendungsposition()
        {
            SendungsanfrageDTO saDTO = new SendungsanfrageDTO();
            saDTO.SaNr = 1;
            SendungspositionDTO sp = new SendungspositionDTO();
            sp.Bruttogewicht = FEU.MAXZULADUNG_TONS + 1; // Es können auf Grund des Gewichtes keine Frachteinheiten erzeugt werden
            saDTO.Sendungspositionen.Add(sp);
            saDTO.AbholzeitfensterStart = DateTime.Parse("29.07.2013");
            saDTO.AbholzeitfensterEnde = DateTime.Parse("04.08.2013");
            saDTO.StartLokation = hamburgLokation.LokNr;
            saDTO.ZielLokation = bremerhavenLokation.LokNr;

            auftragsServicesFürTransportplanungMock.Setup(aus => aus.FindSendungsanfrageEntity(saDTO.SaNr))
                .Returns(saDTO.ToEntity());

            ITransportplanungJob job = transportplanungsServicesFürAuftrag.StarteTransportplanungAsync(saDTO.SaNr);
            job.Wait();
            Assert.IsTrue(job.Status == TransportplanungJobStatusTyp.BeendetNok);
            Assert.IsTrue(job.Meldungen.Count == 1);
            Assert.IsTrue(job.Meldungen[0].Tag == TransportplanungMeldungTag.FrachteinheitenBildungNichtMöglich);
        }

        [TestMethod]
        public void TestPlanungEinSegmentZweiTEUSuccessDreiTEUFailWegenZuGeringerKapazität()
        {
            SendungsanfrageDTO saDTO = new SendungsanfrageDTO();
            saDTO.SaNr = 1;
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

            auftragsServicesFürTransportplanungMock.Setup(aus => aus.FindSendungsanfrageEntity(saDTO.SaNr))
                .Returns(saDTO.ToEntity());

            ITransportplanungJob job = transportplanungsServicesFürAuftrag.StarteTransportplanungAsync(saDTO.SaNr);
            job.Wait();
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

            SendungspositionDTO sp3 = new SendungspositionDTO();
            sp3.Bruttogewicht = TEU.MAXZULADUNG_TONS;
            saDTO.Sendungspositionen.Add(sp3);
            auftragsServicesFürTransportplanungMock.Setup(aus => aus.FindSendungsanfrageEntity(saDTO.SaNr))
               .Returns(saDTO.ToEntity());
            job = transportplanungsServicesFürAuftrag.StarteTransportplanungAsync(saDTO.SaNr);
            job.Wait();
            List<TransportplanDTO> pläne2 = transportplanungsServices.FindTransportplaeneZuSendungsanfrage(saDTO.SaNr);
            Assert.IsTrue(pläne2.Count == 0); // Kapazität des FRV nur zwei TEU
        }

        [TestMethod]
        public void TestPlanungEinSegmentZweiSendungsanfragenSuccessDieDritteFailWegenKeineKapazitätMehrFürZeitfenster()
        {
            SendungsanfrageDTO saDTO1 = new SendungsanfrageDTO();
            saDTO1.SaNr = 1;
            saDTO1.Sendungspositionen.Add(new SendungspositionDTO());
            saDTO1.AbholzeitfensterStart = DateTime.Parse("29.07.2013");
            saDTO1.AbholzeitfensterEnde = DateTime.Parse("04.08.2013");
            saDTO1.StartLokation = hamburgLokation.LokNr;
            saDTO1.ZielLokation = bremerhavenLokation.LokNr;

            auftragsServicesFürTransportplanungMock.Setup(aus => aus.FindSendungsanfrageEntity(saDTO1.SaNr))
                .Returns(saDTO1.ToEntity());

            ITransportplanungJob job = transportplanungsServicesFürAuftrag.StarteTransportplanungAsync(saDTO1.SaNr);
            job.Wait();
            List<TransportplanDTO> pläne1 = transportplanungsServices.FindTransportplaeneZuSendungsanfrage(saDTO1.SaNr);
            Assert.IsTrue(pläne1.Count == 1); // Rest durch TestPlanungEinSegmentSuccess geprüft

            SendungsanfrageDTO saDTO2 = new SendungsanfrageDTO();
            saDTO2.SaNr = 2;
            saDTO2.Sendungspositionen.Add(new SendungspositionDTO());
            saDTO2.AbholzeitfensterStart = DateTime.Parse("30.07.2013");
            saDTO2.AbholzeitfensterEnde = DateTime.Parse("04.08.2013");
            saDTO2.StartLokation = hamburgLokation.LokNr;
            saDTO2.ZielLokation = bremerhavenLokation.LokNr;

            auftragsServicesFürTransportplanungMock.Setup(aus => aus.FindSendungsanfrageEntity(saDTO2.SaNr))
                .Returns(saDTO2.ToEntity());

            job = transportplanungsServicesFürAuftrag.StarteTransportplanungAsync(saDTO2.SaNr);
            job.Wait();
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
            saDTO3.SaNr = 3;
            saDTO3.Sendungspositionen.Add(new SendungspositionDTO());
            saDTO3.AbholzeitfensterStart = DateTime.Parse("29.07.2013");
            saDTO3.AbholzeitfensterEnde = DateTime.Parse("30.07.2013");
            saDTO3.StartLokation = hamburgLokation.LokNr;
            saDTO3.ZielLokation = bremerhavenLokation.LokNr;

            auftragsServicesFürTransportplanungMock.Setup(aus => aus.FindSendungsanfrageEntity(saDTO3.SaNr))
                .Returns(saDTO3.ToEntity());

            job = transportplanungsServicesFürAuftrag.StarteTransportplanungAsync(saDTO3.SaNr);
            job.Wait();
            List<TransportplanDTO> pläne3 = transportplanungsServices.FindTransportplaeneZuSendungsanfrage(saDTO3.SaNr);
            Assert.IsTrue(pläne3.Count == 0); // keine weitere Kapazität für dieses Zeitfenster
        }

        [TestMethod]
        public void TestPlanungEinSegmentFailWegenAbholzeitfensterAußerhalbAbfahrtszeiten()
        {
            SendungsanfrageDTO saDTO = new SendungsanfrageDTO();
            saDTO.SaNr = 1;
            saDTO.Sendungspositionen.Add(new SendungspositionDTO());
            saDTO.AbholzeitfensterStart = DateTime.Parse("27.07.2013");
            saDTO.AbholzeitfensterEnde = DateTime.Parse("29.07.2013");
            saDTO.StartLokation = hamburgLokation.LokNr;
            saDTO.ZielLokation = bremerhavenLokation.LokNr;

            auftragsServicesFürTransportplanungMock.Setup(aus => aus.FindSendungsanfrageEntity(saDTO.SaNr))
                .Returns(saDTO.ToEntity());

            ITransportplanungJob job = transportplanungsServicesFürAuftrag.StarteTransportplanungAsync(saDTO.SaNr);
            job.Wait();
            List<TransportplanDTO> pläne = transportplanungsServices.FindTransportplaeneZuSendungsanfrage(saDTO.SaNr);
            Assert.IsTrue(pläne.Count == 0);
        }

        [TestMethod]
        public void TestPlanungZweiSegmenteSuccess()
        {
            SendungsanfrageDTO saDTO = new SendungsanfrageDTO();
            saDTO.SaNr = 1;
            SendungspositionDTO sp1 = new SendungspositionDTO();
            saDTO.Sendungspositionen.Add(sp1);
            saDTO.AbholzeitfensterStart = DateTime.Parse("29.07.2013");
            saDTO.AbholzeitfensterEnde = DateTime.Parse("04.08.2013");
            saDTO.StartLokation = hamburgLokation.LokNr;
            saDTO.ZielLokation = shanghaiLokation.LokNr;

            auftragsServicesFürTransportplanungMock.Setup(aus => aus.FindSendungsanfrageEntity(saDTO.SaNr))
                .Returns(saDTO.ToEntity());

            ITransportplanungJob job = transportplanungsServicesFürAuftrag.StarteTransportplanungAsync(saDTO.SaNr);
            job.Wait();
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

        [TestMethod]
        public void TestPlanungZweiWegeSuccess()
        {
            SendungsanfrageDTO saDTO = new SendungsanfrageDTO();
            saDTO.SaNr = 1;
            SendungspositionDTO sp1 = new SendungspositionDTO();
            saDTO.Sendungspositionen.Add(sp1);
            saDTO.AbholzeitfensterStart = DateTime.Parse("01.09.2013");
            saDTO.AbholzeitfensterEnde = DateTime.Parse("10.09.2013");
            saDTO.StartLokation = hamburgLokation.LokNr;
            saDTO.ZielLokation = shanghaiLokation.LokNr;

            auftragsServicesFürTransportplanungMock.Setup(aus => aus.FindSendungsanfrageEntity(saDTO.SaNr))
                .Returns(saDTO.ToEntity());

            ITransportplanungJob job = transportplanungsServicesFürAuftrag.StarteTransportplanungAsync(saDTO.SaNr);
            job.Wait();
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

            // HH: 32 Stunden * 10
            // HH->RTD: 2000 + 200
            // RTD: 2 Stunden * 20
            // RTD->SH: 3000 + 300
            Assert.IsTrue(plan2.Kosten == 5860);
        }

        [TestMethod]
        public void TestPlanungZweiWegeWähleTransportplanSuccess()
        {
            SendungsanfrageDTO saDTO = new SendungsanfrageDTO();
            saDTO.SaNr = 7;
            SendungspositionDTO sp1 = new SendungspositionDTO();
            saDTO.Sendungspositionen.Add(sp1);
            saDTO.AbholzeitfensterStart = DateTime.Parse("01.09.2013");
            saDTO.AbholzeitfensterEnde = DateTime.Parse("10.09.2013");
            saDTO.AngebotGültigBis = DateTime.Now.AddHours(1);
            saDTO.StartLokation = hamburgLokation.LokNr;
            saDTO.ZielLokation = shanghaiLokation.LokNr;
            saDTO.Status = SendungsanfrageStatusTyp.Angenommen;

            auftragsServicesFürTransportplanungMock.Setup(aus => aus.FindSendungsanfrageEntity(saDTO.SaNr))
                .Returns(saDTO.ToEntity());

            ITransportplanungJob job = transportplanungsServicesFürAuftrag.StarteTransportplanungAsync(saDTO.SaNr);
            job.Wait();
            
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
            TransportplanDTO planÜberRtd = pläne.Find((plan) =>
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
            Assert.IsTrue(planÜberRtd != null);

            auftragsServicesFürTransportplanungMock.Setup(aus => aus.UpdateSendungsanfrageStatus(saDTO.SaNr, SendungsanfrageStatusTyp.InAusfuehrung))
                .Verifiable();

            Assert.IsTrue(planÜberBhv.TransportplanSchritte.Count == 2);
            pläne = transportplanungsServices.FindTransportplaeneZuSendungsanfrage(saDTO.SaNr);
            Assert.IsTrue(pläne.Count == 2); // es sollten noch beide Pläne vor Wahl eines Plans gespeichert sein
            transportplanungsServicesFuerSendung.FühreTransportplanAus(planÜberBhv.TpNr);
            auftragsServicesFürTransportplanungMock.Verify();
            pläne = transportplanungsServices.FindTransportplaeneZuSendungsanfrage(saDTO.SaNr);
            Assert.IsTrue(pläne.Count == 1, "Plan über Rtd sollte gelöscht sein.");
            Assert.IsTrue(pläne[0].TpNr == planÜberBhv.TpNr, "Plan über Bhv sollte vorhanden sein.");
        }

        [TestMethod]
        public void TestPlanungFailWegenAbholzeitfensterÜberschritten()
        {
            SendungsanfrageDTO saDTO = new SendungsanfrageDTO();
            saDTO.SaNr = 1;
            saDTO.Sendungspositionen.Add(new SendungspositionDTO());
            saDTO.AbholzeitfensterStart = DateTime.Parse("29.07.2013");
            saDTO.AbholzeitfensterEnde = DateTime.Parse("04.08.2013");
            saDTO.StartLokation = hamburgLokation.LokNr;
            saDTO.ZielLokation = bremerhavenLokation.LokNr;

            auftragsServicesFürTransportplanungMock.Setup(aus => aus.FindSendungsanfrageEntity(saDTO.SaNr))
                .Returns(saDTO.ToEntity());
            auftragsServicesFürTransportplanungMock.Setup(aus => aus.UpdateSendungsanfrageStatus(saDTO.SaNr, SendungsanfrageStatusTyp.Unterbrochen))
                .Verifiable();

            ITransportplanungJob job = transportplanungsServicesFürAuftrag.StarteTransportplanungAsync(saDTO.SaNr);
            job.Wait();
            List<TransportplanDTO> pläne = transportplanungsServices.FindTransportplaeneZuSendungsanfrage(saDTO.SaNr);
            Assert.IsTrue(pläne.Count == 1);
            saDTO.Status = SendungsanfrageStatusTyp.Angenommen;
            auftragsServicesFürTransportplanungMock.Setup(aus => aus.FindSendungsanfrageEntity(saDTO.SaNr))
                .Returns(saDTO.ToEntity());
            transportplanungsServicesFuerSendung.FühreTransportplanAus(pläne[0].TpNr);
            auftragsServicesFürTransportplanungMock.Verify();
            pläne = transportplanungsServices.FindTransportplaeneZuSendungsanfrage(saDTO.SaNr);
            Assert.IsTrue(pläne[0].Meldungen.Count == 1);
            Assert.IsTrue(pläne[0].Meldungen[0].Tag == TransportplanMeldungTyp.MeldungTag.AbholzeitUeberschritten);
        }
    }
}
