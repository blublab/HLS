using ApplicationCore.AuftragKomponente.DataAccessLayer;

namespace ApplicationCore.AuftragKomponente.AccessLayer
{
    public interface IAuftragServicesFürTransportplanung
    {
        /// <summary>
        /// Registriert Diensteanbieter für die Transportplanung.
        /// </summary>
        /// <throws>ArgumentException, falls transportplanungsService == null.</throws>
        void RegisterTransportplanungServiceFürAuftrag(ITransportplanungServicesFürAuftrag transportplanungsService);

        /// <summary>
        /// Sucht eine Sendungsanfrage nach SaNr.
        /// </summary>
        /// <returns>Sendungsanfrage; null falls nicht gefunden.</returns>
        /// <throws>ArgumentException, falls saNr <= 0.</throws>
        /// <transaction>Optional</transaction>
        Sendungsanfrage FindSendungsanfrageEntity(int saNr);

        /// <summary>
        /// Setzt einen neuen Status für eine Sendungsanfrage.
        /// </summary>
        /// <throws>ArgumentException, falls saNr <= 0.</throws>
        /// <throws>SendungsanfrageNichtGefundenException</throws>
        /// <transaction>Optional</transaction>
        void UpdateSendungsanfrageStatus(int saNr, SendungsanfrageStatusTyp neuerStatus);
    }
}
