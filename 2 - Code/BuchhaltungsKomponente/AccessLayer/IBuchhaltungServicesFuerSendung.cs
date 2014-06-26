using ApplicationCore.TransportplanungKomponente.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.BuchhaltungKomponente.AccessLayer
{
    public interface IBuchhaltungServicesFuerSendung
    {
        /// <summary>
        /// Erstellt Kundenrechnung
        /// </summary>
        /// <throws>ArgumentException, falls tpNr <= 0</throws>
        /// <throws>ArgumentException, falls SaNr <= 0</throws>
        /// <transaction>Keine aktiven Transaktionen erlaubt.</transaction>
        void ErstelleKundenrechnung(int tpNr, int saNr);
    }
}
