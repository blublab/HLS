using ApplicationCore.AuftragKomponente.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.AuftragKomponente.AccessLayer
{
    public interface IAuftragServicesFuerBuchhaltung
    {
        /// <summary>
        /// Gibt SendungsanfrageDTO für SaNr.
        /// </summary>
        /// <throws>ArgumentException, falls SaNr <= 0</throws>
        /// <transaction>Keine aktiven Transaktionen erlaubt.</transaction>
        SendungsanfrageDTO FindeSendungsanfrageUeberSaNr(int saNr);

        /// <summary>
        /// Schliesst Sendungsanfrage ab.
        /// </summary>
        /// <throws>ArgumentException, falls SaNr <= 0</throws>
        /// <transaction>Keine aktiven Transaktionen erlaubt.</transaction>
        /// <post>Sendungsanfrage ist abgeschlossen</post>
        void SchliesseSendungsanfrageAb(int saNr);
    }
}
