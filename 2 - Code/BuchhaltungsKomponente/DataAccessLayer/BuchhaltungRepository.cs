using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util.PersistenceServices.Interfaces;

namespace ApplicationCore.BuchhaltungKomponente.DataAccessLayer
{
    internal class BuchhaltungRepository
    {
        private readonly IPersistenceServices persistenceService;

        public BuchhaltungRepository(IPersistenceServices persistenceService)
        {
            this.persistenceService = persistenceService;
        }

        public void Save(Frachtabrechnung fab)
        {
            persistenceService.Save(fab);
        }

        public void DeleteFrachtabrechnung(Frachtabrechnung fab)
        {
            persistenceService.Delete<Frachtabrechnung>(fab);
        }

        public void SpeichereKundenrechnung(Kundenrechnung kr)
        {
            persistenceService.Save(kr);
        }

        public Kundenrechnung GetKundenrechnungById(int krNr)
        {
            return persistenceService.GetById<Kundenrechnung, int>(krNr);
        }

        public void SpeichereZahlungseingang(Zahlungseingang ze)
        {
            persistenceService.Save(ze);
        }
    }
}
