using ApplicationCore.AuftragKomponente.DataAccessLayer;
using System;
using System.Collections.Generic;

namespace ApplicationCore.AuftragKomponente.AccessLayer
{
    public interface IAuftragServices
    {
        /// <summary>
        /// Startet die periodische Prüfung der Gültigkeit erfasster Angebote. Ist die Gültigkeit abgelaufen, werden
        /// dier erzeugten Transportpläne gelöscht und der Status der Sendungsanfrage auf "Abgelaufen" gesetzt.
        /// </summary>
        /// <param name="period">Periode der Prüfung.</param>
        /// <throws>ArgumentException, falls Periode nicht mindestens eine Sekunde lang ist.</throws>
        /// <transaction>Nicht erlaubt</transaction>
        void StartAngebotsGültigkeitsPrüfungPeriodicTask(TimeSpan period);

        /// <summary>
        /// Stoppt die periodische Prüfung der Gültigkeit erfasster Angebote.
        /// Kein Effekt, falls die Prüfung nicht gestartet war.
        /// </summary>
        void StopAngebotsGültigkeitsPrüfungPeriodicTask();

        /// <summary>
        /// Sucht eine Sendungsanfrage nach SaNr.
        /// </summary>
        /// <returns>Sendungsanfrage; null, falls nicht gefunden.</returns>
        /// <throws>ArgumentException, falls saNr <= 0.</throws>
        /// <transaction>Nicht erlaubt</transaction>
        SendungsanfrageDTO FindSendungsanfrage(int saNr);

        /// <summary>
        /// Sucht alle Sendungsanfrage.
        /// </summary>
        /// <returns>Sendungsanfragen; null, falls nicht gefunden.</returns>
        /// <transaction>Nicht erlaubt</transaction>
        IList<SendungsanfrageDTO> SelectSendungsanfragen();

        /// <summary>
        /// Erzeugt eine neue Sendungsanfrage.
        /// Nummer der erzeugten Sendungsanfrage wird in SendungsanfrageDTO abgelegt.
        /// </summary>
        /// <throws>ArgumentException, falls saDTO == null.</throws>
        /// <throws>ArgumentException, falls saDTO.SaNr != 0.</throws>
        /// <throws>ArgumentException, falls saDTO.Status != SendungsanfrageStatusTyp.NichtErfasst.</throws>
        /// <transaction>Nicht erlaubt</transaction>
        /// <post>Sendungsanfrage befindet sich im Zustand "Erfasst".</post>
        void CreateSendungsanfrage(ref SendungsanfrageDTO saDTO);

        /// <summary>
        /// Erfasst die Sendungsanfrage und plant asynchron den Transport.
        /// </summary>
        /// <throws>ArgumentException, falls saNr <= 0.</throws>
        /// <throws>SendungsanfrageNichtGefundenException</throws>
        /// <throws>SendungsanfrageNichtErfasstException</throws>
        /// <transaction>Nicht erlaubt</transaction>
        /// <post>Sendungsanfrage befindet sich im Zustand "Geplant".</post>
        List<TransportplanungMeldung> PlaneSendungsanfrage(int saNr);

        /// <summary>
        /// Lehnt eine Sendungsanfrage ab. Die zugehörigen Pläne werden gelöscht.
        /// </summary>
        /// <throws>ArgumentException, falls saNr <= 0.</throws>
        /// <throws>SendungsanfrageNichtGefundenException</throws>
        /// <throws>SendungsanfrageNichtGeplantException</throws>
        /// <transaction>Nicht erlaubt</transaction>
        /// <post>Sendungsanfrage befindet sich im Zustand "Abgelehnt".</post>
        void LehneAngebotAb(int saNr);

        /// <summary>
        /// Nimmt eine Sendungsanfrage mit dem angegebenen Plan an.
        /// </summary>
        /// <throws>ArgumentException, falls saNr <= 0.</throws>
        /// <throws>SendungsanfrageNichtGefundenException</throws>
        /// <throws>SendungsanfrageNichtGeplantException</throws>
        /// <transaction>Nicht erlaubt</transaction>
        /// <post>Sendungsanfrage befindet sich im Zustand "Angenommen".</post>
        void NimmAngebotAn(int saNr);

        /// <summary>
        /// Gibt die Ziellokation für eine Sendungsanfrage zurück.
        /// </summary>
        /// <returns>Ziellokation</returns>
        /// <throws>ArgumentException, falls saNr <= 0.</throws>
        /// <throws>SendungsanfrageNichtGefundenException</throws>
        /// <transaction>Nicht erlaubt</transaction>
        long GibZielLokationFuerSendungsanfrage(int saNr);
    }
}
