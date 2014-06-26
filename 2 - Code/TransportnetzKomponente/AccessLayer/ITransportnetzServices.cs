using ApplicationCore.TransportnetzKomponente.AccessLayer;
using ApplicationCore.TransportnetzKomponente.DataAccessLayer;
using System;
using System.Collections.Generic;

namespace ApplicationCore.TransportnetzKomponente.AccessLayer
{
    public interface ITransportnetzServices
    {
        /// <summary>
        /// Fügt dem Repository eine neue Lokation hinzu.
        /// </summary>
        /// <throws>ArgumentException, falls lokationDTO == null.</throws>
        /// <throws>ArgumentException, falls lokationDTO.LokNr != 0.</throws>
        void CreateLokation(ref LokationDTO lokationDTO);

        /// <summary>
        /// Fügt dem Repository eine neue Transportbeziehung hinzu.
        /// </summary>
        /// <throws>ArgumentException, falls tbDTO == null.</throws>
        /// <throws>ArgumentException, falls tbDTO.TbNr != -1.</throws>
        void CreateTransportbeziehung(ref TransportbeziehungDTO tbDTO);

        /// <summary>
        /// Sucht eine Lokation nach Lokationsnummer.
        /// </summary>
        /// <throws>ArgumentException, lokNr < 0.</throws>
        /// <returns>Lokation zur Lokationsnummer; null, falls keine solche Lokation gefunden.</returns>
        LokationDTO FindLokation(long lokNr);

        /// <summary>
        /// Sucht alle Lokation aus DB.
        /// </summary>
        /// <returns>Lokationen Liste</returns>
        IList<LokationDTO> SelectLokationen();

        /// <summary>
        /// Sucht eine Transportbeziehung nach Transportbeziehungsnummer.
        /// </summary>
        /// <throws>ArgumentException, tbNr < 0.</throws>
        /// <returns>Transportbeziehung zur Transportbeziehungsnummer; null, falls keine solche Transportbeziehung gefunden.</returns>
        TransportbeziehungDTO FindTransportbeziehung(long tbNr);

        /// <summary>
        /// Löscht alle Knoten und Beziehungen aus dem Repository.
        /// </summary>
        void DeleteTransportnetz();
    }
}
