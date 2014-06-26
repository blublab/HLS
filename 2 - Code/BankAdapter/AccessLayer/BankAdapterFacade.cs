using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationCore.BankAdapter.BusinessLogicLayer;
using ApplicationCore.BuchhaltungKomponente.AccessLayer;
using ApplicationCore.BuchhaltungKomponente.DataAccessLayer;
using Common.Implementations;

namespace ApplicationCore.BankAdapter.AccessLayer
{
    public class BankAdapterFacade : IBankServicesFuerBuchhaltung
    {
        private readonly BankAdapterBuisinessLogic ba_BL;

        public BankAdapterFacade()
        {
            this.ba_BL = new BankAdapterBuisinessLogic();
        }

        #region
        void IBankServicesFuerBuchhaltung.SendeGutschriftAnBank(GutschriftDTO gutschrift)
        {
            Check.Argument(gutschrift != null, "gutschrift != null");

            this.ba_BL.SendeGutschriftAnBank(gutschrift);
        }
        #endregion

        public void EmpfangeZahlungseingaengenAusQueue()
        {
            this.ba_BL.EmpfangeZahlungseingaengenAusQueue();
        }

        public void EmpfangeZahlungseingaenge(bool b)
        {
            this.ba_BL.EmpfangeZahlungseingaenge(b);
        }

        public void SetzeBuchhaltungServiceFuerBank(IBuchhaltungsServicesFuerBank buchhaltungsServicesFuerBank)
        {
            this.ba_BL.SetzeBuchhaltungServiceFuerBank(buchhaltungsServicesFuerBank);
        }
    }
}
