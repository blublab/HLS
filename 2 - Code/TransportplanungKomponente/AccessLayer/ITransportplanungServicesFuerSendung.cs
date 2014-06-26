using ApplicationCore.TransportplanungKomponente.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.TransportplanungKomponente.AccessLayer
{
    public interface ITransportplanungServicesFuerSendung
    {
        /// <summary>
        /// Update des Status des Transportplans auf den übergebenen Status.
        /// </summary>
        /// <throws>ArgumentException, falls tpNr <= 0.</throws>
        /// <throws>TransportplanNichtGefundenException</throws>
        /// <transaction>Nicht Erlaubt</transaction>
        /// <post>Transportplan befindet sich im übergebenen Status.</post>
        void UpdateTransportplanstatus(int tpNr, TransportplanStatusTyp tpStTyp);

        /// <summary>
        /// Wählt einen Transportplan aus und beauftragt dessen Auführung an den Frachtführer.
        /// Alle alternativen Transportpläne werden verworfen.
        /// </summary>
        /// <throws>ArgumentException, falls tpNr <= 0.</throws>
        /// <throws>TransportplanNichtGefundenException</throws>
        /// <throws>SendungsanfrageNichtGefundenException, falls zu Transportplan zugeordnete Sendungsanfrage nicht gefunden wurde.</throws>
        /// <throws>SendungsanfrageNichtAngenommenException, falls zu Transportplan zugeordnete Sendungsanfrage nicht angenommen wurde.</throws>
        /// <transaction>Nicht Erlaubt</transaction>
        /// <post>Zugehörige Sendungsanfrage befindet sich im Zustand "In Ausführung".</post>
        void FühreTransportplanAus(int tpNr);
    }
}
