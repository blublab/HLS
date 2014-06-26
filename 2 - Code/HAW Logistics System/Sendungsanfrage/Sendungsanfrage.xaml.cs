using ApplicationCore.AuftragKomponente.AccessLayer;
using ApplicationCore.AuftragKomponente.DataAccessLayer;
using ApplicationCore.GeschaeftspartnerKomponente.AccessLayer;
using ApplicationCore.GeschaeftspartnerKomponente.DataAccessLayer;
using ApplicationCore.TransportnetzKomponente.AccessLayer;
using ApplicationCore.TransportnetzKomponente.DataAccessLayer;
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
using System.Windows.Shapes;
using Util.PersistenceServices.Implementations;
using Util.PersistenceServices.Interfaces;

namespace Client
{
    /// <summary>
    /// Interaktionslogik für Sendungsanfrage.xaml
    /// </summary>
    public partial class GUI_Sendungsanfrage : Window
    {
        private SendungsanfrageDTO saDTO = new SendungsanfrageDTO();

        private IGeschaeftspartnerServices geschaeftspartnerServices = null;
        private IAuftragServices auftragServices = null;
        private ITransportnetzServices transportnetzServices = null;
        private IList<LokationDTO> lokationen = null;

        private int positionenCount = 1;
        public GUI_Sendungsanfrage(ref IAuftragServices auftragServices,ref IGeschaeftspartnerServices geschaeftspartnerServices,ref ITransportnetzServices transportnetzServices, IList<LokationDTO> lokationen)
        {
            InitializeComponent();
            log4net.Config.XmlConfigurator.Configure();

            this.auftragServices = auftragServices;
            this.geschaeftspartnerServices = geschaeftspartnerServices;
            this.transportnetzServices = transportnetzServices;
            this.lokationen = lokationen;

            FillComboBoxMitGeschaeftspartnerAusDB();
            FillComboBoxMitLokationenAusDB();

            Dp_AbholfensterStart.SelectedDate = DateTime.Parse("01.09.2013");
            Dp_AbholfensterEnde.SelectedDate = DateTime.Parse("10.09.2013");
            Dp_AngebotGueltigBis.SelectedDate = DateTime.Now.AddHours(1);
        }

        #region "Geschäftspartner + Adresse"
        private void Btn_NeuenGeschaeftspartner_Click(object sender, RoutedEventArgs e)
        {
            GUI_Geschaeftspartner gp = new GUI_Geschaeftspartner(geschaeftspartnerServices);
            gp.ShowDialog();

            FillComboBoxMitGeschaeftspartnerAusDB();
        }

        private void Btn_NeueAdresse_Click(object sender, RoutedEventArgs e)
        {
            int gpNr = GetSelectedGeschaeftspartnerFromComboBox();

            GUI_Adresse a = new GUI_Adresse(geschaeftspartnerServices, gpNr);
            a.ShowDialog();

            FillComboBoxMitAdressenVonGeschaeftspartner(gpNr);
        }

        private void Cb_gp_changed(object sender, SelectionChangedEventArgs e)
        {
            Cb_ad.Items.Clear();

            if (Cb_gp.SelectedIndex != -1)
            {
                int gpNr = int.Parse(((ComboBoxItem)Cb_gp.SelectedItem).Tag.ToString());
                FillComboBoxMitAdressenVonGeschaeftspartner(gpNr);
            }
        }
        #endregion

        #region "Sendungsposition"
        private void Btn_SendungspositionHinzufuegen_Click(object sender, RoutedEventArgs e)
        {
            decimal positionGewicht = GetBruttogewichtFromTextbox();

            ////BL
            SendungspositionDTO sp = new SendungspositionDTO() { Bruttogewicht = positionGewicht };
            saDTO.Sendungspositionen.Add(sp);

            ////Design
            string position = positionenCount++ + ": " + positionGewicht;
            lb_hinzugefuegtePositionen.Items.Add(new TextBlock() { Text = position });

            this.Height += 20;
        }
        #endregion

        #region "Lokationen"
        private void Btn_NeueLokation_Click(object sender, RoutedEventArgs e)
        {
            GUI_Lokation l = new GUI_Lokation(transportnetzServices);
            l.ShowDialog();

            FillComboBoxMitLokationenAusDB();
        }
        #endregion

        private void Btn_Save_Click(object sender, RoutedEventArgs e)
        {
            saDTO.Auftraggeber = GetSelectedGeschaeftspartnerFromComboBox();
            saDTO.AbholzeitfensterStart = Dp_AbholfensterStart.SelectedDate.Value;
            saDTO.AbholzeitfensterEnde = Dp_AbholfensterEnde.SelectedDate.Value;
            saDTO.AngebotGültigBis = Dp_AngebotGueltigBis.SelectedDate.Value;

            ////TODO:Verify if needed
            //TransportbeziehungDTO tspBezHin = new TransportbeziehungDTO(GetSelectedLokationStart(), GetSelectedLokationZiel());
            //TransportbeziehungDTO tspBezRueck = new TransportbeziehungDTO(GetSelectedLokationZiel(), GetSelectedLokationStart());
            //transportnetzServices.CreateTransportbeziehung(ref tspBezHin);
            //transportnetzServices.CreateTransportbeziehung(ref tspBezRueck);
            ////TODO ENDE

            saDTO.StartLokation = GetSelectedStartLokation().LokNr;
            saDTO.ZielLokation = GetSelectedZielLokation().LokNr;

            auftragServices.CreateSendungsanfrage(ref saDTO);
            auftragServices.PlaneSendungsanfrage(saDTO.SaNr);

            Close();

            MessageBox.Show("Neue Sendungsanfrage gespeichert.", this.Name, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #region "Util Methoden"
        private void FillComboBoxMitGeschaeftspartnerAusDB()
        {
            IList<GeschaeftspartnerDTO> gpList = geschaeftspartnerServices.SelectGeschaeftspartner();

            Cb_gp.Items.Clear();

            foreach (GeschaeftspartnerDTO gp in gpList)
            {
                Cb_gp.Items.Add(new ComboBoxItem() { Content = gp.Vorname + " " + gp.Nachname, Tag = gp.GpNr });
            }

            Cb_ad.IsEnabled = false;
            btn_NeueAdresse.IsEnabled = false;
        }

        private void FillComboBoxMitAdressenVonGeschaeftspartner(int gpID)
        {
            Cb_ad.IsEnabled = true;
            btn_NeueAdresse.IsEnabled = true;

            GeschaeftspartnerDTO gpDTO = geschaeftspartnerServices.FindGeschaeftspartner(gpID);

            Cb_ad.Items.Clear();

            foreach (AdresseDTO ad in gpDTO.Adressen)
            {
                Cb_ad.Items.Add(new ComboBoxItem() { Content = ad.Strasse + " " + ad.Hausnummer + " " + ad.PLZ + " " + ad.Wohnort + " " + ad.Land, Tag = ad.Id });
            }
        }

        private int GetSelectedGeschaeftspartnerFromComboBox()
        {
            try
            {
                return int.Parse(((ComboBoxItem)Cb_gp.SelectedItem).Tag.ToString());
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private decimal GetBruttogewichtFromTextbox()
        {
            try
            {
                return decimal.Parse(tb_sendungsPositionBruttogewicht.Text);
            }
            catch (Exception)
            {
                return 0.0m;
            }
        }

        private void FillComboBoxMitLokationenAusDB()
        {
            Cb_lokStart.Items.Clear();
            Cb_lokZiel.Items.Clear();

            foreach (LokationDTO lok in lokationen)
            {
                Cb_lokStart.Items.Add(new ComboBoxItem() { Content = lok.Name, Tag = lok.LokNr });
                Cb_lokZiel.Items.Add(new ComboBoxItem() { Content = lok.Name, Tag = lok.LokNr });
            }

            Cb_lokStart.SelectedIndex = 0;
            Cb_lokZiel.SelectedIndex = 2;
        }

        private LokationDTO GetSelectedStartLokation()
        {
            long lokNr = long.Parse(((ComboBoxItem)Cb_lokStart.SelectedItem).Tag.ToString());
            LokationDTO result = null;
            foreach (LokationDTO lokDTO in lokationen)
            {
                if (lokDTO.LokNr == lokNr)
                {
                    result = lokDTO;
                }
            }
            return result;
        }

        private LokationDTO GetSelectedZielLokation()
        {
            long lokNr = long.Parse(((ComboBoxItem)Cb_lokZiel.SelectedItem).Tag.ToString());
            LokationDTO result = null;
            foreach (LokationDTO lokDTO in lokationen)
            {
                if (lokDTO.LokNr == lokNr)
                {
                    result = lokDTO;
                }
            }
            return result;
        }
        #endregion
    }
}
