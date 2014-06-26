using ApplicationCore.BuchhaltungKomponente.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.BuchhaltungKomponente.AccessLayer
{
    public interface IBankServicesFuerBuchhaltung
    {
        /// <summary>
        /// Sendet Gutschrift an Bank.
        /// </summary>
        /// <throws> ArgumentException, wenn Gutschrift == null </throws>
        void SendeGutschriftAnBank(GutschriftDTO gutschrift);
    }
}
