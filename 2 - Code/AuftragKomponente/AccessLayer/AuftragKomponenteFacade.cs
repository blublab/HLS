using ApplicationCore.AuftragKomponente.BusinessLogicLayer;
using ApplicationCore.AuftragKomponente.DataAccessLayer;
using Common.Implementations;
using System;
using System.Collections.Generic;
using System.Threading;
using Util.PeriodicTaskFactory;
using Util.PersistenceServices.Interfaces;
using Util.TimeServices;

namespace ApplicationCore.AuftragKomponente.AccessLayer
{
    public class AuftragKomponenteFacade : IAuftragServices, IAuftragServicesFürTransportplanung, IAuftragServicesFuerBuchhaltung
    {
        private readonly SendungsanfrageRepository sa_REPO;
        private readonly ITransactionServices transactionService;
        private readonly AuftragKomponenteBusinessLogic aufK_BL;
        private readonly CancellationTokenSource perdiodicGültigkeitsprüfungTaskCancellationTokenSource;
        private bool transportplanungsServiceInitialized = false;

        public AuftragKomponenteFacade(IPersistenceServices persistenceService, ITransactionServices transactionService, ITimeServices timeServices)
        {
            Check.Argument(persistenceService != null, "persistenceService != null");
            Check.Argument(transactionService != null, "transactionService != null");
            Check.Argument(timeServices != null, "timeServices != null");

            this.sa_REPO = new SendungsanfrageRepository(persistenceService);
            this.aufK_BL = new AuftragKomponenteBusinessLogic(this.sa_REPO, timeServices);
            this.transactionService = transactionService;
            this.perdiodicGültigkeitsprüfungTaskCancellationTokenSource = new CancellationTokenSource();
        }

        #region IAuftragServices
        public void StartAngebotsGültigkeitsPrüfungPeriodicTask(TimeSpan period)
        {
            Check.OperationCondition(this.transportplanungsServiceInitialized == true, "AuftragsKomponente wurde nicht korrekt initialisiert. Rückverbindung zur Komponente TransportPlanung muss hergestellt sein (Methode RegisterTransportplanungServiceFürAuftrag).");
            Check.Argument(period.TotalSeconds >= 1, "Periode muss mindestens eine Sekunde lang sein.");
            Check.OperationCondition(!transactionService.IsTransactionActive, "Keine aktive Transaktion erlaubt.");

            PeriodicTaskFactory.Start(
                () =>
                {
                    transactionService.ExecuteTransactional(() =>
                    {
                        this.aufK_BL.PrüfeAngebotsGültigkeit();
                    });
                }, 
                intervalInMilliseconds: (int)period.TotalMilliseconds, 
                synchronous: true,
                cancelToken: perdiodicGültigkeitsprüfungTaskCancellationTokenSource.Token); 
        }

        public void StopAngebotsGültigkeitsPrüfungPeriodicTask()
        {
            Check.OperationCondition(this.transportplanungsServiceInitialized == true, "AuftragsKomponente wurde nicht korrekt initialisiert. Rückverbindung zur Komponente TransportPlanung muss hergestellt sein (Methode RegisterTransportplanungServiceFürAuftrag).");
            
            perdiodicGültigkeitsprüfungTaskCancellationTokenSource.Cancel();
        }

        public SendungsanfrageDTO FindSendungsanfrage(int saNr)
        {
            Check.OperationCondition(this.transportplanungsServiceInitialized == true, "AuftragsKomponente wurde nicht korrekt initialisiert. Rückverbindung zur Komponente TransportPlanung muss hergestellt sein (Methode RegisterTransportplanungServiceFürAuftrag).");
            Check.OperationCondition(!transactionService.IsTransactionActive, "Keine aktive Transaktion erlaubt.");
            Check.Argument(saNr > 0, "SaNr muss größer als 0 sein.");

            Sendungsanfrage sa = null;
            transactionService.ExecuteTransactional(() => 
                { 
                    sa = this.sa_REPO.FindBySaNr(saNr);
                });

            if (sa == null)
            {
                return null;
            }

            return sa.ToDTO();
        }

        public IList<SendungsanfrageDTO> SelectSendungsanfragen()
        {
            IList<SendungsanfrageDTO> saDTOList = new List<SendungsanfrageDTO>();
            IList<Sendungsanfrage> saList = new List<Sendungsanfrage>();

            transactionService.ExecuteTransactional(() =>
            {
                saList = this.sa_REPO.Select();
            });

            foreach (Sendungsanfrage sa in saList)
            {
                saDTOList.Add(sa.ToDTO());
            }

            return saDTOList;
        }

        public void CreateSendungsanfrage(ref SendungsanfrageDTO saDTO)
        {
            Check.OperationCondition(this.transportplanungsServiceInitialized == true, "AuftragsKomponente wurde nicht korrekt initialisiert. Rückverbindung zur Komponente TransportPlanung muss hergestellt sein (Methode RegisterTransportplanungServiceFürAuftrag).");
            Check.OperationCondition(!transactionService.IsTransactionActive, "Keine aktive Transaktion erlaubt."); 
            Check.Argument(saDTO != null, "saDTO != null");
            Check.Argument(saDTO.SaNr == 0, "saDTO.SaNr == 0");
            Check.Argument(saDTO.Status == SendungsanfrageStatusTyp.NichtErfasst, "saDTO.Status == SendungsanfrageStatusTyp.NichtErfasst");
            
            Sendungsanfrage sa = saDTO.ToEntity();
            transactionService.ExecuteTransactional(() => 
                { 
                    this.aufK_BL.UpdateSendungsanfrageStatus(sa, SendungsanfrageStatusTyp.Erfasst);
                    this.sa_REPO.Save(sa);
                });
            saDTO = sa.ToDTO();
        }

        public List<TransportplanungMeldung> PlaneSendungsanfrage(int saNr)
        {
            Check.OperationCondition(this.transportplanungsServiceInitialized == true, "AuftragsKomponente wurde nicht korrekt initialisiert. Rückverbindung zur Komponente TransportPlanung muss hergestellt sein (Methode RegisterTransportplanungServiceFürAuftrag).");
            Check.OperationCondition(!transactionService.IsTransactionActive, "Keine aktive Transaktion erlaubt.");
            Check.Argument(saNr > 0, "SaNr muss größer als 0 sein.");
            Sendungsanfrage sa = CheckSendungsanfrageVorhanden(saNr);
            if (sa.Status != SendungsanfrageStatusTyp.Erfasst)
            {
                throw new SendungsanfrageNichtErfasstException(saNr);
            }

            return this.aufK_BL.PlaneSendungsanfrage(saNr);
        }

       public void NimmAngebotAn(int saNr)
        {
            Check.OperationCondition(this.transportplanungsServiceInitialized == true, "AuftragsKomponente wurde nicht korrekt initialisiert. Rückverbindung zur Komponente TransportPlanung muss hergestellt sein (Methode RegisterTransportplanungServiceFürAuftrag).");
            Check.OperationCondition(!transactionService.IsTransactionActive, "Keine aktive Transaktion erlaubt.");
            Check.Argument(saNr > 0, "SaNr muss größer als 0 sein.");
            Sendungsanfrage sa = CheckSendungsanfrageVorhanden(saNr);
            if (sa.Status != SendungsanfrageStatusTyp.Geplant)
            {
                throw new SendungsanfrageNichtGeplantException(saNr);
            }

            transactionService.ExecuteTransactional(() =>
            {
                sa = this.FindSendungsanfrageEntity(saNr);
                this.aufK_BL.NehmeAngebotAn(sa);
            });
        }

        public void LehneAngebotAb(int saNr)
        {
            Check.OperationCondition(this.transportplanungsServiceInitialized == true, "AuftragsKomponente wurde nicht korrekt initialisiert. Rückverbindung zur Komponente TransportPlanung muss hergestellt sein (Methode RegisterTransportplanungServiceFürAuftrag).");
            Check.OperationCondition(!transactionService.IsTransactionActive, "Keine aktive Transaktion erlaubt.");
            Check.Argument(saNr > 0, "SaNr muss größer als 0 sein.");
            Sendungsanfrage sa = CheckSendungsanfrageVorhanden(saNr);
            if (sa.Status != SendungsanfrageStatusTyp.Geplant)
            {
                throw new SendungsanfrageNichtGeplantException(saNr);
            }

            transactionService.ExecuteTransactional(
                () =>
                {
                    sa = this.sa_REPO.FindBySaNr(saNr);
                    Check.Argument(sa.Status == SendungsanfrageStatusTyp.Geplant, "sa.Status == SendungsanfrageStatus.Geplant");
                    this.aufK_BL.LehneAngebotAb(sa);
                });
        }

        public long GibZielLokationFuerSendungsanfrage(int saNr)
        {
            Check.OperationCondition(!transactionService.IsTransactionActive, "Keine aktive Transaktion erlaubt.");
            Check.Argument(saNr > 0, "SaNr muss größer als 0 sein.");
            Sendungsanfrage sa = CheckSendungsanfrageVorhanden(saNr);
            return this.aufK_BL.GibZielLokationFuerSendungsanfrage(sa);
        }
        #endregion

        #region IAuftragServicesFürTransportplanung
        public void RegisterTransportplanungServiceFürAuftrag(ITransportplanungServicesFürAuftrag transportplanungsService)
        {
            Check.Argument(transportplanungsService != null, "transportplanungsService != null");

            this.aufK_BL.RegisterTransportplanungServiceFürAuftrag(transportplanungsService);
            transportplanungsServiceInitialized = true;
        }

        public Sendungsanfrage FindSendungsanfrageEntity(int saNr)
        {
            Check.OperationCondition(this.transportplanungsServiceInitialized == true, "AuftragsKomponente wurde nicht korrekt initialisiert. Rückverbindung zur Komponente TransportPlanung muss hergestellt sein (Methode RegisterTransportplanungServiceFürAuftrag).");
            Check.Argument(saNr > 0, "SaNr muss größer als 0 sein.");

            return transactionService.ExecuteTransactionalIfNoTransactionProvided(
               () => { return this.sa_REPO.FindBySaNr(saNr); });
        }

        public void UpdateSendungsanfrageStatus(int saNr, SendungsanfrageStatusTyp neuerStatus)
        {
            Check.OperationCondition(this.transportplanungsServiceInitialized == true, "AuftragsKomponente wurde nicht korrekt initialisiert. Rückverbindung zur Komponente TransportPlanung muss hergestellt sein (Methode RegisterTransportplanungServiceFürAuftrag).");
            Check.Argument(saNr > 0, "SaNr muss größer als 0 sein.");
            Sendungsanfrage sa = CheckSendungsanfrageVorhanden(saNr);

            transactionService.ExecuteTransactionalIfNoTransactionProvided(() =>
            {
                this.aufK_BL.UpdateSendungsanfrageStatus(sa, neuerStatus);
            });
        }
        #endregion

        private Sendungsanfrage CheckSendungsanfrageVorhanden(int saNr)
        {
            Sendungsanfrage sa = null;
            transactionService.ExecuteTransactionalIfNoTransactionProvided(() =>
            {
                sa = this.sa_REPO.FindBySaNr(saNr);
                if (sa == null)
                {
                    throw new SendungsanfrageNichtGefundenException(saNr);
                }
            });
            return sa;
        }

        #region IAuftragServicesFuerBuchhaltung
        public SendungsanfrageDTO FindeSendungsanfrageUeberSaNr(int saNr)
        {
            Check.OperationCondition(this.transportplanungsServiceInitialized == true, "AuftragsKomponente wurde nicht korrekt initialisiert. Rückverbindung zur Komponente TransportPlanung muss hergestellt sein (Methode RegisterTransportplanungServiceFürAuftrag).");
            Check.OperationCondition(!transactionService.IsTransactionActive, "Keine aktive Transaktion erlaubt.");
            Check.Argument(saNr > 0, "SaNr muss größer als 0 sein.");

            Sendungsanfrage sa = null;
            transactionService.ExecuteTransactional(() =>
            {
                sa = this.sa_REPO.FindBySaNr(saNr);
            });

            if (sa == null)
            {
                return null;
            }

            return sa.ToDTO();
        }

        public void SchliesseSendungsanfrageAb(int saNr)
        {
            Check.OperationCondition(!transactionService.IsTransactionActive, "Keine aktive Transaktion erlaubt.");
            Check.Argument(saNr > 0, "SaNr muss größer als 0 sein.");

            transactionService.ExecuteTransactionalIfNoTransactionProvided(() =>
            {
                Sendungsanfrage sa = this.sa_REPO.FindBySaNr(saNr);
                this.aufK_BL.UpdateSendungsanfrageStatus(sa, SendungsanfrageStatusTyp.Abgeschlossen);
                this.sa_REPO.Save(sa);
            });
        }
        #endregion
    }
}
