using ApplicationCore.AuftragKomponente.AccessLayer;
using ApplicationCore.AuftragKomponente.DataAccessLayer;
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
    /// Interaktionslogik für Status.xaml
    /// </summary>
    public partial class Status : Window
    {
        private SendungsanfrageDTO saDTO = null;
        private IAuftragServices auftragServices = null;

        public Status(IAuftragServices auftragServices, SendungsanfrageDTO saDTO)
        {
            InitializeComponent();
            this.auftragServices = auftragServices;

            this.saDTO = saDTO;

            tb_id.Text = "Status für Sendungsanfrage " + saDTO.SaNr.ToString() + " ändern:";
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Btn_Annehmen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                auftragServices.NimmAngebotAn(saDTO.SaNr);
                MessageBox.Show("Angebot angenommen", this.Name, MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Btn_Ablehnen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                auftragServices.LehneAngebotAb(saDTO.SaNr);
                MessageBox.Show("Angebot abgelehnt.", this.Name, MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
