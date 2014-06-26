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

namespace Client
{
    /// <summary>
    /// Interaktionslogik für Lokation.xaml
    /// </summary>
    public partial class GUI_Lokation : Window
    {
        private ITransportnetzServices transportnetzServices = null;
        public GUI_Lokation(ITransportnetzServices transportnetzServices)
        {
            InitializeComponent();
            this.transportnetzServices = transportnetzServices;
        }

        private void Btn_Save_Click(object sender, RoutedEventArgs e)
        {
            LokationDTO lok = new LokationDTO(
                Tb_name.Text.Trim(),
                TimeSpan.Parse(Tb_maxLagerZeit.Text.Trim()),
                decimal.Parse(Tb_lagerKostenProStunde.Text.Trim()));

            transportnetzServices.CreateLokation(ref lok);

            Close();

            MessageBox.Show("Neue Lokation " + Tb_name.Text.Trim() + " gespeichert.", this.Name, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
