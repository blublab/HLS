using ApplicationCore.AuftragKomponente.AccessLayer;
using ApplicationCore.AuftragKomponente.DataAccessLayer;
using ApplicationCore.TransportnetzKomponente.AccessLayer;
using ApplicationCore.TransportplanungKomponente.BusinessLogicLayer;
using ApplicationCore.TransportplanungKomponente.DataAccessLayer;
using ApplicationCore.UnterbeauftragungKomponente.AccessLayer;
using Common.Implementations;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Util.PersistenceServices.Interfaces;
using Util.TimeServices;

namespace ApplicationCore.TransportplanungKomponente.AccessLayer
{
    public class TransportplanungKomponenteFacade : ITransportplanungServices, ITransportplanungServicesFürAuftrag, ITransportplanungServicesFuerSendung, ITransportplanServicesFuerBuchhaltung
    {
        private readonly ITransactionServices transactionService;
        private readonly IAuftragServicesFürTransportplanung auftragServices;
        private readonly TransportplanungKomponenteBusinessLogic tpK_BL;
        private readonly TransportplanRepository tp_REPO;

        public TransportplanungKomponenteFacade(IPersistenceServices persistenceService, ITransactionServices transactionService, IAuftragServicesFürTransportplanung auftragServices, IUnterbeauftragungServicesFürTransportplanung unterbeauftragungServices, ITransportnetzServicesFürTransportplanung transportnetzServices, ITimeServices timeServices)
        {
            Check.Argument(persistenceService != null, "persistenceService != null");
            Check.Argument(transactionService != null, "transactionService != null");
            Check.Argument(auftragServices != null, "auftragServices != null");
            Check.Argument(unterbeauftragungServices != null, "unterbeauftragungsServices != null");
            Check.Argument(transportnetzServices != null, "transportnetzServices != null");

            this.transactionService = transactionService;
            this.auftragServices = auftragServices;
            this.tp_REPO = new TransportplanRepository(persistenceService);
            this.tpK_BL = new TransportplanungKomponenteBusinessLogic(tp_REPO, transactionService, auftragServices, unterbeauftragungServices, transportnetzServices, timeServices);
        }

        #region ITransportplanungsServices
        public List<TransportplanDTO> FindTransportplaeneZuSendungsanfrage(int saNr)
        {
            Check.Argument(saNr > 0, "SaNr muss größer als 0 sein.");

            return transactionService.ExecuteTransactionalIfNoTransactionProvided(
               () => 
               {
                   return this.tp_REPO.SucheZuSendungsanfrage(saNr).ConvertAll<TransportplanDTO>((tp) => { return tp.ToDTO(); }); 
               });
        }

        public void SaveTransportplan(ref TransportplanDTO tpDTO)
        {
            Transportplan tp = tpDTO.ToEntity();
            transactionService.ExecuteTransactional(() =>
            {
                this.tp_REPO.Save(tp);
            });
        }

        public List<TransportplanDTO> SelectTransportplaene()
        {
            return transactionService.ExecuteTransactionalIfNoTransactionProvided(
               () =>
               {
                   return this.tp_REPO.Select().ConvertAll<TransportplanDTO>((tp) => { return tp.ToDTO(); });
               });
        }
        #endregion

        #region ITransportplanungServicesFürAuftrag
        public ITransportplanungJob StarteTransportplanungAsync(int saNr)
        {
            Check.Argument(saNr > 0, "saNr > 0");

            return this.tpK_BL.StarteTransportplanungAsync(saNr);
        }

        public void LöscheTransportpläneAsync(int saNr)
        {
            Check.Argument(saNr > 0, "saNr > 0");

            this.tpK_BL.LöscheTransportpläneAsync(saNr);
        }
        #endregion

        #region ITransportplanungServicesFuerSendung
        public void UpdateTransportplanstatus(int tpNr, TransportplanStatusTyp tpStTyp)
        {
            Check.Argument(tpNr > 0, "TpNr muss größer als 0 sein.");
            Check.OperationCondition(!transactionService.IsTransactionActive, "Keine aktive Transaktion erlaubt.");

            transactionService.ExecuteTransactional(() =>
            {
                Transportplan gewählterPlan = this.tp_REPO.FindByTpNr(tpNr);
                if (gewählterPlan == null)
                {
                    throw new TransportplanNichtGefundenException(gewählterPlan.TpNr);
                }
                this.tpK_BL.UpdateTransportplanstatus(gewählterPlan, tpStTyp);
            });
        }

        public void FühreTransportplanAus(int tpNr)
        {
            Check.Argument(tpNr > 0, "TpNr muss größer als 0 sein.");
            Check.OperationCondition(!transactionService.IsTransactionActive, "Keine aktive Transaktion erlaubt.");

            transactionService.ExecuteTransactional(() =>
            {
                Transportplan gewählterPlan = this.tp_REPO.FindByTpNr(tpNr);
                if (gewählterPlan == null)
                {
                    throw new TransportplanNichtGefundenException(gewählterPlan.TpNr);
                }
                Sendungsanfrage sa = auftragServices.FindSendungsanfrageEntity(gewählterPlan.SaNr);
                if (sa == null)
                {
                    throw new SendungsanfrageNichtGefundenException(gewählterPlan.SaNr);
                }
                if (sa.Status != SendungsanfrageStatusTyp.Angenommen)
                {
                    throw new SendungsanfrageNichtAngenommenException(gewählterPlan.SaNr);
                }

                this.tpK_BL.FühreTransportplanAus(gewählterPlan, sa);
            });
        }
        #endregion

        #region ITransportplanServicesFuerBuchhaltung
        public TransportplanDTO FindeTransportplanUeberTpNr(int tpNr)
        {
            Check.Argument(tpNr > 0, "tpNr > 0");
            Check.OperationCondition(!transactionService.IsTransactionActive, "Keine aktive Transaktion erlaubt.");

            return transactionService.ExecuteTransactional(() =>
            {
                Transportplan gewählterPlan = this.tp_REPO.FindByTpNr(tpNr);
                return gewählterPlan.ToDTO();
            });
        }
        #endregion
    }
}
