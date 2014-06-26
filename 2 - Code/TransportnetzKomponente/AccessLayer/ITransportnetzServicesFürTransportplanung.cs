using ApplicationCore.TransportnetzKomponente.DataAccessLayer;
using System;
using System.Collections.Generic;

namespace ApplicationCore.TransportnetzKomponente.AccessLayer
{
    public interface ITransportnetzServicesFürTransportplanung
    {
        /// <summary>
        /// Generiert alle Pfade von der Quell- zur Ziellokation.
        /// </summary>
        /// <returns>Liste von Pfaden. Leere Liste, falls keine Pfade existieren.</returns>
        /// <throws>LokationNichtVorhandenException</throws>
        List<List<Transportbeziehung>> GeneriereAllePfadeVonBis(long startLokation, long zielLokation);
    }
}
