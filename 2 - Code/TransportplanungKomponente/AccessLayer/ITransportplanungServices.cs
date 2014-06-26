using ApplicationCore.AuftragKomponente.DataAccessLayer;
using ApplicationCore.TransportplanungKomponente.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicationCore.TransportplanungKomponente.AccessLayer
{
    public interface ITransportplanungServices
    {
        /// <summary>
        /// Liefert eine Liste der erzeugten Transportpläne, falls verfügbar.
        /// </summary>
        /// <returns>Transportpläne; leere Liste, falls keine Pläne vorhanden.</returns>
        /// <throws>ArgumentException, falls saNr <= 0.</throws>
        /// <throws>SendungsanfrageNichtGefundenException</throws>
        /// <transaction>Optional</transaction>
        List<TransportplanDTO> FindTransportplaeneZuSendungsanfrage(int saNr);

        /// <summary>
        /// Speichert Transportplan.
        /// </summary>
        /// <throws>ArgumentException, falls tpDTO == null</throws>
        void SaveTransportplan(ref TransportplanDTO tpDTO);

        /// <summary>
        /// Liefert eine Liste der erzeugten Transportpläne, falls verfügbar.
        /// </summary>
        /// <returns>Transportpläne; leere Liste, falls keine Pläne vorhanden.</returns>
        /// <transaction>Optional</transaction>
        List<TransportplanDTO> SelectTransportplaene();
    }
}
