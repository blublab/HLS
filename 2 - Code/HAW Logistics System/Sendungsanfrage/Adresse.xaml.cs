using ApplicationCore.GeschaeftspartnerKomponente.AccessLayer;
using ApplicationCore.GeschaeftspartnerKomponente.DataAccessLayer;
using Common.DataTypes;
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
using Util.PersistenceServices.Implementations;
using Util.PersistenceServices.Interfaces;

namespace Client
{
    /// <summary>
    /// Interaktionslogik für Adresse.xaml
    /// </summary>
    public partial class GUI_Adresse : Window
    {
        private IGeschaeftspartnerServices geschaeftspartnerServices = null;

        private int gpNr = 0;

        public GUI_Adresse(IGeschaeftspartnerServices geschaeftspartnerServices, int gpNr)
        {
            InitializeComponent();
            this.gpNr = gpNr;

            log4net.Config.XmlConfigurator.Configure();

            this.geschaeftspartnerServices = geschaeftspartnerServices;
        }

        private void Btn_Save_Click(object sender, RoutedEventArgs e)
        {
            AdresseDTO kundenadresse = new AdresseDTO()
            {
                Strasse = Tb_street.Text.Trim(),
                Hausnummer = Tb_Num.Text.Trim(),
                Land = Tb_country.Text.Trim(),
                PLZ = Tb_zip.Text.Trim(),
                Wohnort = Tb_city.Text.Trim()
            };

            GeschaeftspartnerDTO gpDTO = geschaeftspartnerServices.FindGeschaeftspartner(gpNr);

            gpDTO.Adressen.Add(kundenadresse);

            geschaeftspartnerServices.UpdateGeschaeftspartner(ref gpDTO);

            MessageBox.Show("Neue Adresse gespeichert.", this.Name, MessageBoxButton.OK, MessageBoxImage.Information);

            Close();
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
