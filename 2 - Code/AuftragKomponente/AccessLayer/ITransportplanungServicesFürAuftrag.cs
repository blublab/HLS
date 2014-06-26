using ApplicationCore.AuftragKomponente.DataAccessLayer;
using System;
using System.Threading.Tasks;

namespace ApplicationCore.AuftragKomponente.AccessLayer
{
    public interface ITransportplanungServicesFürAuftrag
    {
        /// <summary>
        /// Startet die asynchrone Transportplanung.
        /// </summary>
        /// <pre>Gültige Sendungsanfrage.</pre>
        ITransportplanungJob StarteTransportplanungAsync(int saNr);

        /// <summary>
        /// Startet das asynchrone Löschen aller Transportpläne.
        /// </summary>
        /// <pre>Gültige Sendungsanfrage.</pre>
        void LöscheTransportpläneAsync(int saNr);
    }
}
