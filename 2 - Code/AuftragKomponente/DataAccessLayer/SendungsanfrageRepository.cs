using Common.Implementations;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Util.PersistenceServices.Interfaces;

namespace ApplicationCore.AuftragKomponente.DataAccessLayer
{
    internal class SendungsanfrageRepository
    {
        private readonly IPersistenceServices persistenceService;

        public SendungsanfrageRepository(IPersistenceServices persistenceService)
        {
            Check.Argument(persistenceService != null, "persistenceService != null");

            this.persistenceService = persistenceService;
        }

        public void Save(Sendungsanfrage sa)
        {
            persistenceService.Save(sa);
        }

        public Sendungsanfrage FindBySaNr(int saNr)
        {
            return persistenceService.GetById<Sendungsanfrage, int>(saNr);
        }

        public List<Sendungsanfrage> FindGeplant()
        {
            List<Sendungsanfrage> lsa =
               (from sa in persistenceService.Query<Sendungsanfrage>()
                where sa.Status == SendungsanfrageStatusTyp.Geplant
                select sa).ToList();

            return lsa;
        }

        public IList<Sendungsanfrage> Select()
        {
            return persistenceService.GetAll<Sendungsanfrage>();
        }
    }
}
