using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.SendungKomponente.AccessLayer
{
    public interface ISendungServices
    {
        /// <summary>
        /// Erstellt eine Sendung
        /// </summary>
        /// <throws>ArgumentException, falls tpNr kleinergleich 0.</throws>     
        /// <throws>ArgumentException, falls saNr kleinergleich 0.</throws>
        /// <transaction>Nicht erlaubt</transaction>
        void ErstelleSendung(int tpNr, int saNr);
    }
}
