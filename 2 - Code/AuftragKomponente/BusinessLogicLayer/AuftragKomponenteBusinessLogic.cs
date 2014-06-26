using ApplicationCore.AuftragKomponente.AccessLayer;
using ApplicationCore.AuftragKomponente.DataAccessLayer;
using Common.Implementations;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using Util.PersistenceServices.Interfaces;
using Util.TimeServices;

namespace ApplicationCore.AuftragKomponente.BusinessLogicLayer
{
    internal class AuftragKomponenteBusinessLogic
    {
        private readonly SendungsanfrageRepository sa_REPO;
        private readonly ITimeServices timeServices;

        private ITransportplanungServicesFürAuftrag transportPlanungservice = null;

        internal AuftragKomponenteBusinessLogic(SendungsanfrageRepository sa_REPO, ITimeServices timeServices)
        {
            Check.Argument(sa_REPO != null, "sa_REPO != null");
            Check.Argument(timeServices != null, "timeServices != null");

            this.sa_REPO = sa_REPO;
            this.timeServices = timeServices;
        }

        public void RegisterTransportplanungServiceFürAuftrag(ITransportplanungServicesFürAuftrag transportplanungsService)
        {
            Contract.Requires(transportplanungsService != null);
            Contract.Requires(this.transportPlanungservice == null);

            this.transportPlanungservice = transportplanungsService;
        }

        internal void PrüfeAngebotsGültigkeit()
        {
            Contract.Requires(this.transportPlanungservice != null);

            List<Sendungsanfrage> lsa = this.sa_REPO.FindGeplant();
            lsa.ForEach((sa) =>
            {
                if (DateTime.Now > sa.AngebotGültigBis)
                {
                    this.transportPlanungservice.LöscheTransportpläneAsync(sa.SaNr);
                    sa.UpdateStatus(SendungsanfrageStatusTyp.Abgelaufen);
                }
            });
        }

        internal void LehneAngebotAb(Sendungsanfrage sa)
        {
            Contract.Requires(sa != null);
            Contract.Requires(sa.Status == SendungsanfrageStatusTyp.Geplant);
            Contract.Requires(this.transportPlanungservice != null);

            sa.UpdateStatus(SendungsanfrageStatusTyp.Abgelehnt);
            this.transportPlanungservice.LöscheTransportpläneAsync(sa.SaNr);
        }

        internal List<TransportplanungMeldung> PlaneSendungsanfrage(int saNr)
        {
            Contract.Requires(saNr > 0);
            Contract.Requires(this.transportPlanungservice != null);

            ITransportplanungJob job = this.transportPlanungservice.StarteTransportplanungAsync(saNr);
            job.Wait();

            return job.Meldungen;
        }

        public void UpdateSendungsanfrageStatus(Sendungsanfrage sa, SendungsanfrageStatusTyp neuerStatus)
        {
            Contract.Requires(sa  == null);
            Contract.Requires(sa.SaNr > 0);

            sa.UpdateStatus(neuerStatus);
        }

        public void NehmeAngebotAn(Sendungsanfrage sa)
        {
            Contract.Requires(sa == null);
            Contract.Requires(sa.SaNr > 0);

            if (sa.Status != SendungsanfrageStatusTyp.Geplant)
            {
                throw new ArgumentException("Sendungsanfrage kann nicht angenommen werden, da sie sich im Zustand '" + sa.Status.ToString() + "' befindet.");
            }
            sa.UpdateStatus(SendungsanfrageStatusTyp.Angenommen);
        }

        public long GibZielLokationFuerSendungsanfrage(Sendungsanfrage sa)
        {
            Contract.Requires(sa == null);
            Contract.Requires(sa.SaNr > 0);

            return sa.ZielLokation;
        }
    }
}
