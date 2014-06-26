using ApplicationCore.TransportplanungKomponente.AccessLayer;
using ApplicationCore.TransportplanungKomponente.DataAccessLayer;
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
    /// Interaction logic for TransportplanStatus.xaml
    /// </summary>
    public partial class TransportplanStatus : Window
    {
        private TransportplanDTO tpDTO = null;
        private ITransportplanungServicesFuerSendung transportplanungServicesFuerSendung = null;

        public TransportplanStatus(
            ITransportplanungServicesFuerSendung transportplanungServicesFuerSendung, 
            TransportplanDTO tpDTO)
        {
            InitializeComponent();
            this.tpDTO = tpDTO;
            this.transportplanungServicesFuerSendung = transportplanungServicesFuerSendung;
            tb_id.Text = "Transportplan " + tpDTO.TpNr.ToString() + " ausführen?";
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btn_Ausfuehren_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                transportplanungServicesFuerSendung.FühreTransportplanAus(tpDTO.TpNr);
                MessageBox.Show("Transportplan wird ausgeführt.", this.Name, MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
