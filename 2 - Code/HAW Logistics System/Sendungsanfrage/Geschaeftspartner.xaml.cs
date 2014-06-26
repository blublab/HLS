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
using System.Windows.Shapes;
using Util.PersistenceServices.Implementations;
using Util.PersistenceServices.Interfaces;

namespace Client
{
    /// <summary>
    /// Interaktionslogik für Geschaeftspartner.xaml
    /// </summary>
    public partial class GUI_Geschaeftspartner : Window
    {
        private  IGeschaeftspartnerServices geschaeftspartnerServices = null;

        public GUI_Geschaeftspartner(IGeschaeftspartnerServices geschaeftspartnerServices)
        {
            InitializeComponent();
            log4net.Config.XmlConfigurator.Configure();

            this.geschaeftspartnerServices = geschaeftspartnerServices;
        }

        private void Btn_Save_Click(object sender, RoutedEventArgs e)
        {
            GeschaeftspartnerDTO gpDTO = new GeschaeftspartnerDTO()
            {
                Adressen = new List<AdresseDTO>(),
                Email = new EMailType(Tb_email.Text.Trim()),
                Vorname = Tb_firstName.Text.Trim(),
                Nachname = Tb_lastName.Text.Trim(),
            };
            geschaeftspartnerServices.CreateGeschaeftspartner(ref gpDTO);
   
            MessageBox.Show("Neuen Geschäftspartner gespeichert.", this.Name, MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
