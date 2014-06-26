using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util.PersistenceServices.Interfaces;

namespace ApplicationCore.SendungKomponente.DataAccessLayer
{
    internal class SendungRepository
    {
        private readonly IPersistenceServices persistenceService;

        public SendungRepository(IPersistenceServices persistenceService)
        {
            this.persistenceService = persistenceService;
        }

        public void Save(Sendung s)
        {
            persistenceService.Save(s);
        }

        public Sendung FindBySNr(int sNr)
        {
            return persistenceService.GetById<Sendung, int>(sNr);
        }
    }
}
