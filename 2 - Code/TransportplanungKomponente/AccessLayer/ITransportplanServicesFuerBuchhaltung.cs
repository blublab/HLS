using ApplicationCore.TransportplanungKomponente.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.TransportplanungKomponente.AccessLayer
{
    public interface ITransportplanServicesFuerBuchhaltung
    {
        /// <summary>
        /// Gibt TransportplanDTO zur übergebenen tpNr.
        /// </summary>
        /// <throws>ArgumentException, falls tpNr <=0 </throws>
        /// <transactions>Keine aktiven Transaktionen erlaubt.</transactions>
        TransportplanDTO FindeTransportplanUeberTpNr(int tpNr);
    }
}
