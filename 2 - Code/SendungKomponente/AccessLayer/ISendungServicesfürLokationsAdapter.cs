using ApplicationCore.SendungKomponente.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.SendungKomponente.AccessLayer
{
    public interface ISendungServicesfürLokationsAdapter
    {
        /// <summary>
        /// Verarbeitet aus der Queue gelesenes Sendungsverfolgungsereignis.
        /// </summary>
        /// <throws>ArgumentException, falls sveDTO = null</throws>
        void VerarbeiteSendungsverfolgungsereignis(ref SendungsverfolgungsereignisDTO sveDTO);
    }
}
