using System;
using System.Collections.Generic;
using ApplicationCore.UnterbeauftragungKomponente.DataAccessLayer;

namespace ApplicationCore.UnterbeauftragungKomponente.AccessLayer
{
    public interface IUnterbeauftragungServicesFürTransportplanung
    {
        /// <summary>
        /// Sucht den für eine Transportbeziehung innerhalb eines Zeitraums gültige Verträge.
        /// </summary>
        /// <returns>Verträge; leere Liste, falls keine gültigen Verträge gefunden wurden.</returns>
        /// <throws>ArgumentException, falls tbNr <= 0.</throws>
        /// <throws>ArgumentException, falls zeitspanneVon > zeitspanneBis.</throws>
        /// <transaction>Optional</transaction>
        List<FrachtfuehrerRahmenvertrag> FindGültigFür(long tbNr, DateTime zeitspanneVon, DateTime zeitspanneBis);

        /// <summary>
        /// Beauftragt einen Transport im Rahmen eines gültigen FrachtführerRahmenvertrags.
        /// </summary>
        /// <returns>Nummer des erzeugten Frachtaufrags.</returns>
        /// <throws>ArgumentException, falls frvNr <= 0.</throws>
        /// <throws>ArgumentException, falls PlanStartzeit > PlanEndezeit.</throws>
        /// <throws>ArgumentException, falls VerwendeteKapazitaetTEU < 0.</throws>
        /// <throws>ArgumentException, falls VerwendeteKapazitaetFEU < 0.</throws>
        /// <throws>ArgumentException, falls VerwendeteKapazitaetTEU + VerwendeteKapazitaetFEU == 0.</throws>
        /// <throws>ArgumentException, falls saNr <= 0.</throws>
        /// <throws>FrachtfuehrerRahmenvertragNichtGefundenException</throws>
        /// <transaction>Optional</transaction>
        int BeauftrageTransport(int frvNr, DateTime planStartzeit, DateTime planEndezeit, int verwendeteKapazitaetTEU, int verwendeteKapazitaetFEU, int saNr);
    }
}
