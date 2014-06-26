using ApplicationCore.BuchhaltungKomponente.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.BuchhaltungKomponente.AccessLayer
{
    public interface IBuchhaltungsServicesFuerBank
    {
        /// <summary>
        /// Verarbeitet den Zahlungseigang.
        /// </summary>
        /// <throws>ArgumentException, falls zeDTO == null</throws>
        void VerarbeiteZahlungseingang(ref ZahlungseingangDTO zeDTO);
    }
}
