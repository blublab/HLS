using ApplicationCore.UnterbeauftragungKomponente.DataAccessLayer;
using System;
using System.Collections.Generic;

namespace ApplicationCore.UnterbeauftragungKomponente.AccessLayer
{
    public interface IFrachtfuehrerServicesFürUnterbeauftragung
    {
        /// <summary>
        /// Beauftragt einen Transport im Rahmen eines gültigen FrachtführerRahmenvertrags.
        /// </summary>
        /// <transaction>Never</transaction>
        void SendeFrachtauftragAnFrachtfuehrer(FrachtauftragDTO fraDTO);
    }
}
