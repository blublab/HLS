using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationCore.BuchhaltungKomponente.DataAccessLayer;
using ApplicationCore.UnterbeauftragungKomponente.DataAccessLayer;

namespace ApplicationCore.BuchhaltungKomponente.AccessLayer
{
    public interface IBuchhaltungServices 
    {
        /// <summary>
        /// Erzeugt eine Frachtabrechnung Entität.
        /// </summary>
        /// <throws>ArgumenException, falls FrachtauftragDTO == null.</throws>
        /// <transaction>Nicht erlaubt</transaction>
        void CreateFrachtabrechnung(ref FrachtauftragDTO faufDTO);

        /// <summary>
        /// Estellt Gutschrift und sendet an BankAdapter.
        /// </summary>
        /// <throws>ArgumentException, falls FrachtauftragDTO == null</throws>
        /// <post>Frachtauftrag befindet sich im Zustand "Abgeschlossen".</post>
        void PayFrachtabrechnung(ref FrachtabrechnungDTO fabDTO);

        /// <summary>
        /// Löscht Frachtabrechnung und ggf. Gutschrift.
        /// </summary>
        /// <throws>ArgumentException, falls FrachtauftragDTO == null</throws>
        void DeleteFrachtabrechnung(ref FrachtabrechnungDTO fabDTO);
    }
}
