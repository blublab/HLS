using Common.Implementations;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Util.PersistenceServices.Interfaces;

namespace ApplicationCore.TransportplanungKomponente.DataAccessLayer
{
    internal class TransportplanRepository
    {
        private readonly IPersistenceServices persistenceService;

        public TransportplanRepository(IPersistenceServices persistenceService)
        {
            Check.Argument(persistenceService != null, "persistenceService != null");

            this.persistenceService = persistenceService;
        }

        public void Save(Transportplan tp)
        {
            Contract.Requires(tp != null);
            Contract.Requires(tp.TpNr > 0);

            persistenceService.Save(tp);
        }

        public void Delete(Transportplan tp)
        {
            Contract.Requires(tp != null);
            Contract.Requires(tp.TpNr > 0);

            persistenceService.Delete(tp);
        }

        public void Delete(Frachteinheit frae)
        {
            Contract.Requires(frae != null);
            Contract.Requires(frae.FraeNr > 0);

            persistenceService.Delete(frae);
        }

        public Transportplan FindByTpNr(int tpNr)
        {
            Contract.Requires(tpNr > 0);

            return persistenceService.GetById<Transportplan, int>(tpNr);
        }

        public List<TransportAktivitaet> FindZuVertragUndStartzeit(int frachtführerRahmenvertragNr, DateTime startzeit)
        {
            List<TransportAktivitaet> lta =
                (from ta in persistenceService.Query<TransportAktivitaet>()
                 where ta.FrachtfuehrerRahmenvertrag == frachtführerRahmenvertragNr && ta.PlanStartzeit == startzeit
                 select ta).ToList();

            return lta;
        }

        public List<Transportplan> SucheZuSendungsanfrage(int saNr)
        {
            List<Transportplan> ltp =
                (from tp in persistenceService.Query<Transportplan>()
                 where tp.SaNr == saNr
                 select tp).ToList();

            return ltp;
        }

        public List<Transportplan> Select()
        {
            List<Transportplan> ltp =
                    (from tp in persistenceService.Query<Transportplan>()
                     select tp).ToList();

            return ltp;
        }
    }
}
