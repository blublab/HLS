using ApplicationCore.AuftragKomponente.AccessLayer;
using ApplicationCore.AuftragKomponente.DataAccessLayer;
using ApplicationCore.BuchhaltungKomponente.AccessLayer;
using ApplicationCore.FrachtfuehrerAdapter.AccessLayer;
using ApplicationCore.GeschaeftspartnerKomponente.AccessLayer;
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Util.PersistenceServices.Implementations;
using Util.PersistenceServices.Interfaces;
using Util.TimeServices;

namespace Client
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static IPersistenceServices persistenceService = null;
        private static ITransactionServices transactionService = null;
        private static IUnterbeauftragungServices unterbeauftragungsServices = null;
        private static ITransportplanungServicesFuerSendung transportplanungServicesFuerSendung = null;

        private static IAuftragServices auftragService = null;
        private static IGeschaeftspartnerServices geschaeftspartnerService = null;
        private static ITransportnetzServices transportnetzService = null;
        private static ITransportplanungServicesFürAuftrag transportplanungServicesFürAuftrag = null;
        private static ITransportplanungServices transportplanungServices = null;
        private IList<LokationDTO> lokationen = null;

        public MainWindow()
        {
            InitializeComponent();

            PersistenceServicesFactory.CreateSimpleMySQLPersistenceService(out persistenceService, out transactionService);

            var timeServicesMock = new Mock<ITimeServices>();
            //// Wir müssen einen fixen Zeitpunkt simulieren, ansonsten sind bei der Ausführung/Planung evtl. die Verträge oder Angebote abgelaufen
            timeServicesMock.Setup(ts => ts.Now).Returns(DateTime.Parse("31.08.2013 12:00"));

            auftragService = new AuftragKomponenteFacade(persistenceService, transactionService, timeServicesMock.Object);
            geschaeftspartnerService = new GeschaeftspartnerKomponenteFacade(persistenceService, transactionService);
            transportnetzService = new TransportnetzKomponenteFacade();
            IAuftragServicesFürTransportplanung auftragsServicesFürTransportplanung = auftragService as IAuftragServicesFürTransportplanung;

            IBuchhaltungServices buchhaltungServices = new BuchhaltungKomponenteFacade(
                persistenceService,
                transactionService,
                new Mock<IUnterbeauftragungServicesFuerBuchhaltung>().Object,
                new Mock<IBankServicesFuerBuchhaltung>().Object,
                new Mock<ITransportplanServicesFuerBuchhaltung>().Object,
                new Mock<IAuftragServicesFuerBuchhaltung>().Object,
                geschaeftspartnerService,
                new Mock<IPDFErzeugungsServicesFuerBuchhaltung>().Object);
            IFrachtfuehrerServicesFürUnterbeauftragung frachtfuehrerServices = new FrachtfuehrerAdapterFacade(ref buchhaltungServices);

            IUnterbeauftragungServicesFürTransportplanung unterbeauftragungServicesFürTransportplanung = new UnterbeauftragungKomponenteFacade(
                persistenceService,
                transactionService,
                frachtfuehrerServices);

            ITransportnetzServicesFürTransportplanung transportnetzServicesFürTransportplanung = new TransportnetzKomponenteFacade();

            transportplanungServicesFürAuftrag = new TransportplanungKomponenteFacade(
                persistenceService,
                transactionService,
                auftragsServicesFürTransportplanung,
                unterbeauftragungServicesFürTransportplanung,
                transportnetzServicesFürTransportplanung,
                timeServicesMock.Object);
            auftragsServicesFürTransportplanung.RegisterTransportplanungServiceFürAuftrag(transportplanungServicesFürAuftrag);

            transportplanungServices = new TransportplanungKomponenteFacade(
                persistenceService,
                transactionService,
                auftragsServicesFürTransportplanung,
                unterbeauftragungServicesFürTransportplanung,
                transportnetzServicesFürTransportplanung,
                timeServicesMock.Object);

            transportplanungServicesFuerSendung = new TransportplanungKomponenteFacade(
                persistenceService,
                transactionService,
                auftragsServicesFürTransportplanung,
                unterbeauftragungServicesFürTransportplanung,
                transportnetzServicesFürTransportplanung,
                timeServicesMock.Object); 

            unterbeauftragungsServices = new UnterbeauftragungKomponenteFacade(persistenceService, transactionService, frachtfuehrerServices);

            ConfigStart();
        }

        private void ConfigStart()
        {
            LokationDTO hamburgLokation = new LokationDTO("Hamburg", TimeSpan.Parse("10"), 10);
            LokationDTO bremerhavenLokation = new LokationDTO("Bremerhaven", TimeSpan.Parse("15"), 15);
            LokationDTO shanghaiLokation = new LokationDTO("Shanghai", TimeSpan.Parse("10"), 10);

            transportnetzService.CreateLokation(ref hamburgLokation);
            transportnetzService.CreateLokation(ref bremerhavenLokation);
            transportnetzService.CreateLokation(ref shanghaiLokation);

            TransportbeziehungDTO hh_bhv = new TransportbeziehungDTO(hamburgLokation, bremerhavenLokation);
            TransportbeziehungDTO bhv_sh = new TransportbeziehungDTO(bremerhavenLokation, shanghaiLokation);

            lokationen = new List<LokationDTO>();
            lokationen.Add(hamburgLokation);
            lokationen.Add(bremerhavenLokation);
            lokationen.Add(shanghaiLokation);

            transportnetzService.CreateTransportbeziehung(ref hh_bhv);
            transportnetzService.CreateTransportbeziehung(ref bhv_sh);


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
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Btn_NeueSendungsanfrage_Click(object sender, RoutedEventArgs e)
        {
            GUI_Sendungsanfrage sa = new GUI_Sendungsanfrage(ref auftragService, ref geschaeftspartnerService, ref transportnetzService, lokationen);
            sa.ShowDialog();

            FillSendungsanfragen();
            FillTransportplaene();
        }

        private void Dg_sa_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SendungsanfrageDTO saDTO = (SendungsanfrageDTO)dg_sa.SelectedItem;
            Status status = new Status(auftragService, saDTO);

            status.ShowDialog();

            FillSendungsanfragen();
            FillTransportplaene();
        }

        private void FillSendungsanfragen()
        {
            IList<SendungsanfrageDTO> saList = auftragService.SelectSendungsanfragen();
            dg_sa.ItemsSource = saList;
        }

        private void FillTransportplaene()
        {
            List<TransportplanDTO> tpList = transportplanungServices.SelectTransportplaene();
            dg_tp.ItemsSource = tpList;
        }

        private void dg_tp_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TransportplanDTO tpDTO = (TransportplanDTO)dg_tp.SelectedItem;
            TransportplanStatus tpStatus = new TransportplanStatus(transportplanungServicesFuerSendung, tpDTO);

            tpStatus.ShowDialog();
            FillTransportplaene();
        }
    }
}
