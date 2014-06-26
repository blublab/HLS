using ApplicationCore.LokationsAdapter.BuisinessLogicLayer;
using ApplicationCore.SendungKomponente.AccessLayer;
using ApplicationCore.TransportnetzKomponente.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.LokationsAdapter.AccessLayer
{
    public class LokationsApaterFacade
    {
        private readonly LokationsAdapterBuisinessLogic lkA_BL;

        public LokationsApaterFacade(ISendungServicesfürLokationsAdapter sendungServicesfürLokationsAdapter, IList<LokationDTO> lokationen)
        {
            this.lkA_BL = new LokationsAdapterBuisinessLogic(sendungServicesfürLokationsAdapter, lokationen);
        }

        public void EmpfangeSendungsverfolgungsereignisAusQueue()
        {
            this.lkA_BL.SetEmpfangeSendungsverfolgungsereignis(true);
            this.lkA_BL.StarteEmpfangVonSendungsverfolgungsereignis();
        }

        public void SetEmpfangeSendungsverfolgungsereignis(bool b)
        {
            lkA_BL.SetEmpfangeSendungsverfolgungsereignis(b);
        }
    }
}
