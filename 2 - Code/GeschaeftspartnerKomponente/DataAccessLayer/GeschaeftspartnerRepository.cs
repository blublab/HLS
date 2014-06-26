using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Util.PersistenceServices.Interfaces;

namespace ApplicationCore.GeschaeftspartnerKomponente.DataAccessLayer
{
    internal class GeschaeftspartnerRepository
    {
        private readonly IPersistenceServices persistenceService;

        public GeschaeftspartnerRepository(IPersistenceServices persistenceService)
        {
            this.persistenceService = persistenceService;
        }

        public void Save(Geschaeftspartner gp)
        {
            persistenceService.Save(gp);
        }

        public Geschaeftspartner FindByGpNr(int gpNr)
        {
            return persistenceService.GetById<Geschaeftspartner, int>(gpNr);
        }

        public Adresse FindAdrById(int id)
        {
            return persistenceService.GetById<Adresse, int>(id);
        }

        public IList<Geschaeftspartner> Select()
        {
            return persistenceService.GetAll<Geschaeftspartner>();
        }
    }
}
