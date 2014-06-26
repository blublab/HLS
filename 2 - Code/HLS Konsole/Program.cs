using ApplicationCore.AuftragKomponente.AccessLayer;
using ApplicationCore.AuftragKomponente.DataAccessLayer;
using ApplicationCore.BankAdapter;
using ApplicationCore.BankAdapter.AccessLayer;
using ApplicationCore.BuchhaltungKomponente.AccessLayer;
using ApplicationCore.BuchhaltungKomponente.DataAccessLayer;
using ApplicationCore.FrachtfuehrerAdapter;
using ApplicationCore.FrachtfuehrerAdapter.AccessLayer;
using ApplicationCore.GeschaeftspartnerKomponente.AccessLayer;
using ApplicationCore.GeschaeftspartnerKomponente.DataAccessLayer;
using ApplicationCore.PDFErzeugungsKomponente.AccesLayer;
using ApplicationCore.TransportnetzKomponente.AccessLayer;
using ApplicationCore.TransportnetzKomponente.DataAccessLayer;
using ApplicationCore.TransportplanungKomponente.AccessLayer;
using ApplicationCore.TransportplanungKomponente.DataAccessLayer;
using ApplicationCore.UnterbeauftragungKomponente.AccessLayer;
using ApplicationCore.UnterbeauftragungKomponente.DataAccessLayer;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util.PersistenceServices.Implementations;
using Util.PersistenceServices.Interfaces;
using Util.TimeServices;

namespace Client.HLS_Konsole
{
    public class Program
    {
        private static IPersistenceServices persistenceService = null;
        private static ITransactionServices transactionService = null;

        /// <summary>
        /// Schnittstellen der AuftragKomponente
        /// </summary>
        private static IAuftragServices auftragServices = null;
        private static IAuftragServicesFürTransportplanung auftragServicesFürTransportplanung = null;
        private static ITransportplanungServicesFürAuftrag transportplanungServicesFuerAuftrag = null;

        /// <summary>
        /// Schnittstellen der BuchhaltungKomponente
        /// </summary>
        private static IBankServicesFuerBuchhaltung bankServicesFuerBuchhaltung = null;
        private static IBuchhaltungServices buchhaltungsServices = null;

        /// <summary>
        /// Schnittstellen der GeschaeftspartnerKomponente
        /// </summary>
        private static IGeschaeftspartnerServices geschaeftspartnerServices = null;

        /// <summary>
        /// Schnittstellen der TransportnetzKomponente
        /// </summary>
        private static ITransportnetzServices transportnetzServices = null;
        private static ITransportnetzServicesFürTransportplanung transportnetzServicesFuerTransportplanung = null;

        /// <summary>
        /// Schnittstellen der TransportplanungKomponente
        /// </summary>
        private static ITransportplanungServices transportplanungServices = null;
        private static ITransportnetzServicesFürTransportplanung transportnetzServicesFürTransportplanung = null;

        /// <summary>
        /// Schnittstellen der UnterbeauftragungKomponente
        /// </summary>
        private static IUnterbeauftragungServices unterbeauftragungServices = null;
        private static IFrachtfuehrerServicesFürUnterbeauftragung frachtfuehrerServicesFuerUnterbeauftragung = null;
        private static IUnterbeauftragungServicesFuerBuchhaltung unterbeauftragungServicesFuerBuchhaltung = null;
        private static IUnterbeauftragungServicesFürTransportplanung unterbeauftragungServicesFürTransportplanung = null;

        public static void Main(string[] args)
        {
            Console.WriteLine("Initializiere...");
            Init();
            Console.WriteLine("Initializierung abgeschlossen.");
            Console.WriteLine("Befuelle Datenbank...");
            BefuelleDatenbank();
            Console.WriteLine("Datenbank wurde mit Testdaten befuellt.");
            FrachtfuehrerAdapterFacade frachtfuehrerAdapterFacade = new FrachtfuehrerAdapterFacade(ref buchhaltungsServices);
            Console.WriteLine("Empfange Frachtabrechnungen aus MQ und sende Gutschriften an Bank (zum Beenden 'Strg + C')");
            Console.WriteLine("");
            frachtfuehrerAdapterFacade.StarteEmpfangvonFrachabrechnungen();
        }

        /// <summary>
        /// Initializiere Schnittstellen der Komponenten
        /// </summary>
        private static void Init()
        {
            PersistenceServicesFactory.CreateSimpleMySQLPersistenceService(out persistenceService, out transactionService);

            var timeServicesMock = new Mock<ITimeServices>();
            //// Wir müssen einen fixen Zeitpunkt simulieren, ansonsten sind bei der Ausführung/Planung evtl. die Verträge oder Angebote abgelaufen
            timeServicesMock.Setup(ts => ts.Now).Returns(DateTime.Parse("31.08.2013 12:00"));

            auftragServices = new AuftragKomponenteFacade(persistenceService, transactionService, timeServicesMock.Object);

            unterbeauftragungServicesFuerBuchhaltung = new UnterbeauftragungKomponenteFacade(persistenceService, transactionService, frachtfuehrerServicesFuerUnterbeauftragung);
            bankServicesFuerBuchhaltung = new BankAdapterFacade();
            geschaeftspartnerServices = new GeschaeftspartnerKomponenteFacade(persistenceService, transactionService);
            IPDFErzeugungsServicesFuerBuchhaltung pDFErzeugungsServicesFuerBuchhaltung = new PDFErzeugungKomponenteFacade(geschaeftspartnerServices as IGeschaeftspartnerServicesFuerPDFErzeugung); 
            frachtfuehrerServicesFuerUnterbeauftragung = new FrachtfuehrerAdapterFacade(ref buchhaltungsServices);
            unterbeauftragungServices = new UnterbeauftragungKomponenteFacade(persistenceService, transactionService, frachtfuehrerServicesFuerUnterbeauftragung);
            buchhaltungsServices = new BuchhaltungKomponenteFacade(
                persistenceService,
                transactionService,
                unterbeauftragungServicesFuerBuchhaltung,
                bankServicesFuerBuchhaltung,
                transportplanungServicesFuerAuftrag as ITransportplanServicesFuerBuchhaltung,
                auftragServices as IAuftragServicesFuerBuchhaltung,
                geschaeftspartnerServices,
                pDFErzeugungsServicesFuerBuchhaltung);

            auftragServicesFürTransportplanung = new AuftragKomponenteFacade(persistenceService, transactionService, timeServicesMock.Object);
            transportnetzServicesFuerTransportplanung = new TransportnetzKomponenteFacade();
            unterbeauftragungServicesFürTransportplanung = new UnterbeauftragungKomponenteFacade(persistenceService, transactionService, frachtfuehrerServicesFuerUnterbeauftragung);
            transportplanungServicesFuerAuftrag = new TransportplanungKomponenteFacade(
                persistenceService,
                transactionService,
                auftragServicesFürTransportplanung,
                unterbeauftragungServicesFürTransportplanung,
                transportnetzServicesFuerTransportplanung,
                timeServicesMock.Object);
            auftragServicesFürTransportplanung = new AuftragKomponenteFacade(persistenceService, transactionService, timeServicesMock.Object);
            transportnetzServices = new TransportnetzKomponenteFacade();
            transportnetzServicesFürTransportplanung = new TransportnetzKomponenteFacade();
            transportplanungServices = new TransportplanungKomponenteFacade(persistenceService, transactionService, auftragServicesFürTransportplanung, unterbeauftragungServicesFürTransportplanung, transportnetzServicesFuerTransportplanung, timeServicesMock.Object);
        }

        private static void BefuelleDatenbank()
        {
            IPersistenceServices persistenceService = null;
            ITransactionServices transactionService = null;

            ITransportplanungServices transportplanungsServices = null;
            IAuftragServices auftragsServices = null;
            IUnterbeauftragungServices unterbeauftragungsServices = null;
            ITransportnetzServices transportnetzServices = null;
            IFrachtfuehrerServicesFürUnterbeauftragung frachtfuehrerServices = null;

            LokationDTO hamburgLokation;
            LokationDTO bremerhavenLokation;
            LokationDTO shanghaiLokation;
            TransportbeziehungDTO hh_bhv;
            TransportbeziehungDTO bhv_sh;

            log4net.Config.XmlConfigurator.Configure();

            PersistenceServicesFactory.CreateSimpleMySQLPersistenceService(out persistenceService, out transactionService);

            var timeServicesMock = new Mock<ITimeServices>();
            //// Wir müssen einen fixen Zeitpunkt simulieren, ansonsten sind bei der Ausführung/Planung evtl. die Verträge oder Angebote abgelaufen
            timeServicesMock.Setup(ts => ts.Now)
               .Returns(DateTime.Parse("31.08.2013 12:00"));

            transportnetzServices = new TransportnetzKomponenteFacade();
            auftragsServices = new AuftragKomponenteFacade(persistenceService, transactionService, timeServicesMock.Object);
            IAuftragServicesFürTransportplanung auftragsServicesFürTransportplanung = auftragsServices as IAuftragServicesFürTransportplanung;
            ////frachtfuehrerServices = new FrachtfuehrerAdapterFacade();
            unterbeauftragungsServices = new UnterbeauftragungKomponenteFacade(persistenceService, transactionService, frachtfuehrerServices);
            transportplanungsServices = new TransportplanungKomponenteFacade(persistenceService, transactionService, auftragsServicesFürTransportplanung, unterbeauftragungsServices as IUnterbeauftragungServicesFürTransportplanung, transportnetzServices as ITransportnetzServicesFürTransportplanung, timeServicesMock.Object);
            auftragsServicesFürTransportplanung.RegisterTransportplanungServiceFürAuftrag(transportplanungsServices as ITransportplanungServicesFürAuftrag);

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

            SendungsanfrageDTO saDTO = new SendungsanfrageDTO();
            SendungspositionDTO sp1 = new SendungspositionDTO();
            saDTO.Sendungspositionen.Add(sp1);
            saDTO.AbholzeitfensterStart = DateTime.Parse("01.09.2013");
            saDTO.AbholzeitfensterEnde = DateTime.Parse("10.09.2013");
            saDTO.AngebotGültigBis = DateTime.Now.AddHours(1);
            saDTO.StartLokation = hamburgLokation.LokNr;
            saDTO.ZielLokation = shanghaiLokation.LokNr;

            auftragsServices.CreateSendungsanfrage(ref saDTO);
            auftragsServices.PlaneSendungsanfrage(saDTO.SaNr);
            List<TransportplanDTO> pläne = transportplanungsServices.FindTransportplaeneZuSendungsanfrage(saDTO.SaNr);

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
            for (int i = 0; i < 5; i++)
            {
                FrachtauftragDTO faDTO = new FrachtauftragDTO() { FrachtfuehrerRahmenvertrag = frv_bhv_sh, PlanEndezeit = new DateTime(), PlanStartzeit = new DateTime(), VerwendeteKapazitaetFEU = 5, VerwendeteKapazitaetTEU = 5 };
                unterbeauftragungServices.SpeichereFrachtauftrag(ref faDTO);
            }
        }
    }
}
