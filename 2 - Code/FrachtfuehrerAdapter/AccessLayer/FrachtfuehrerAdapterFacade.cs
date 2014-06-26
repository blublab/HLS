using ApplicationCore.BuchhaltungKomponente.AccessLayer;
using ApplicationCore.BuchhaltungKomponente.DataAccessLayer;
using ApplicationCore.FrachtfuehrerAdapter.BusinessLogicLayer;
using ApplicationCore.UnterbeauftragungKomponente.AccessLayer;
using ApplicationCore.UnterbeauftragungKomponente.DataAccessLayer;
using Common.Implementations;
using System;
using Util.MessagingServices.Implementations;
using Util.MessagingServices.Interfaces;
using Util.PersistenceServices.Interfaces;

namespace ApplicationCore.FrachtfuehrerAdapter.AccessLayer
{
    public class FrachtfuehrerAdapterFacade : IFrachtfuehrerServicesFürUnterbeauftragung
    {
        private readonly FrachtfuehrerAdapterBusinessLogic ffA_BL;

        public FrachtfuehrerAdapterFacade(ref IBuchhaltungServices buchhaltungServices)
        {
            this.ffA_BL = new FrachtfuehrerAdapterBusinessLogic(ref buchhaltungServices);
        }

        public void SendeFrachtauftragAnFrachtfuehrer(FrachtauftragDTO fraDTO)
        {
            Check.Argument(fraDTO != null, "fraDTO != null");
            this.ffA_BL.SendeFrachtauftragAnFrachtfuehrer(fraDTO);
        }
        
        public void StarteEmpfangvonFrachabrechnungen()
        {
            this.ffA_BL.StarteEmpfangvonFrachabrechnungen();
        }
    }
}
