using System;
using System.Collections.Generic;
using System.Linq;
using Util.PersistenceServices.Interfaces;

namespace ApplicationCore.UnterbeauftragungKomponente.DataAccessLayer
{
    internal class FrachtfuehrerRahmenvertragRepository
    {
        private readonly IPersistenceServices persistenceService;

        public FrachtfuehrerRahmenvertragRepository(IPersistenceServices persistenceService)
        {
            this.persistenceService = persistenceService;
        }

        public void Save(FrachtfuehrerRahmenvertrag frv)
        {
            persistenceService.Save(frv);
        }

        public FrachtfuehrerRahmenvertrag FindByFrvNr(int frvNr)
        {
            return persistenceService.GetById<FrachtfuehrerRahmenvertrag, int>(frvNr);
        }

        public List<FrachtfuehrerRahmenvertrag> FindGültigFür(long tbNr, DateTime zeitspanneVon, DateTime zeitspanneBis)
        {
            return (from frv in persistenceService.Query<FrachtfuehrerRahmenvertrag>()
                    where frv.FuerTransportAufTransportbeziehung == tbNr &&
                          ((zeitspanneVon >= frv.GueltigkeitAb && zeitspanneVon <= frv.GueltigkeitBis) ||
                           (zeitspanneBis >= frv.GueltigkeitAb && zeitspanneBis <= frv.GueltigkeitBis) ||
                           (zeitspanneVon <= frv.GueltigkeitAb && zeitspanneBis >= frv.GueltigkeitBis))
                    select frv).ToList();
        }
    }
}
