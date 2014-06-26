using ApplicationCore.GeschaeftspartnerKomponente.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.GeschaeftspartnerKomponente.AccessLayer
{
    public interface IGeschaeftspartnerServicesFuerPDFErzeugung
    {
        /// <summary>
        /// Gibt die Adresse zu einer ID zurück.
        /// </summary>
        /// <throws>ArgumentException, falls id <= 0</throws>
        AdresseDTO FindeAdresseZuID(int id);
    }
}
