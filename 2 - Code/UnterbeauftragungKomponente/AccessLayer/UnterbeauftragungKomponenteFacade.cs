using ApplicationCore.UnterbeauftragungKomponente.AccessLayer;
using ApplicationCore.UnterbeauftragungKomponente.BusinessLogicLayer;
using ApplicationCore.UnterbeauftragungKomponente.DataAccessLayer;
using Common.Implementations;
using System;
using System.Collections.Generic;
using Util.PersistenceServices.Interfaces;

namespace ApplicationCore.UnterbeauftragungKomponente.AccessLayer
{
    public class UnterbeauftragungKomponenteFacade : IUnterbeauftragungServices, IUnterbeauftragungServicesFürTransportplanung, IUnterbeauftragungServicesFuerBuchhaltung
    {
        private readonly ITransactionServices transactionService;
        private readonly FrachtfuehrerRahmenvertragRepository frv_REPO;
        private readonly FrachtfuehrerRepository frf_REPO;
        private readonly FrachtauftragRepository fra_REPO;
        private readonly UnterbeauftragungKomponenteBusinessLogic ubK_BL;

        public UnterbeauftragungKomponenteFacade(IPersistenceServices persistenceService, ITransactionServices transactionService, IFrachtfuehrerServicesFürUnterbeauftragung frachtfuehrerServices)
        {
            Check.Argument(persistenceService != null, "persistenceService != null");
            Check.Argument(transactionService != null, "transactionService != null");
            this.transactionService = transactionService;
            this.frv_REPO = new FrachtfuehrerRahmenvertragRepository(persistenceService);
            this.frf_REPO = new FrachtfuehrerRepository(persistenceService);
            this.fra_REPO = new FrachtauftragRepository(persistenceService);
            this.ubK_BL = new UnterbeauftragungKomponenteBusinessLogic(persistenceService, frachtfuehrerServices); 
        }

        public void CreateFrachtfuehrerRahmenvertrag(ref FrachtfuehrerRahmenvertragDTO frvDTO)
        {
            Check.Argument(frvDTO != null, "frvDTO != null");
            Check.Argument(frvDTO.FrvNr == 0, "frvDTO.FrfNr == 0");
            Check.OperationCondition(!transactionService.IsTransactionActive, "Keine aktive Transaktion erlaubt.");
            
            FrachtfuehrerRahmenvertrag frv = frvDTO.ToEntity();
            transactionService.ExecuteTransactional(
                () =>
                {
                    this.frv_REPO.Save(frv);
                });
            frvDTO = frv.ToDTO();
        }

        public void CreateFrachtfuehrer(ref FrachtfuehrerDTO frfDTO)
        {
            Check.Argument(frfDTO != null, "frfDTO != null");
            Check.Argument(frfDTO.FrfNr == 0, "frfDTO.FrfNr == 0");
            Check.OperationCondition(!transactionService.IsTransactionActive, "Keine aktive Transaktion erlaubt.");

            Frachtfuehrer frf = frfDTO.ToEntity();
            transactionService.ExecuteTransactional(
                () =>
                {
                    this.frf_REPO.Save(frf);
                });
            frfDTO = frf.ToDTO();
        }

        public List<FrachtfuehrerRahmenvertrag> FindGültigFür(long tbNr, DateTime zeitspanneVon, DateTime zeitspanneBis)
        {
            Check.Argument(tbNr >= 0, "tbNr >= 0");
            Check.Argument(zeitspanneVon <= zeitspanneBis, "zeitspanneVon <= zeitspanneBis");

            return transactionService.ExecuteTransactionalIfNoTransactionProvided(
                () => 
                {
                    return this.frv_REPO.FindGültigFür(tbNr, zeitspanneVon, zeitspanneBis);
                });
        }

        public int BeauftrageTransport(int frvNr, DateTime planStartzeit, DateTime planEndezeit, int verwendeteKapazitaetTEU, int verwendeteKapazitaetFEU, int saNr)
        {
            Check.Argument(frvNr > 0, "frvNr > 0");
            Check.Argument(planStartzeit <= planEndezeit, "planStartzeit <= planEndezeit");
            Check.Argument(verwendeteKapazitaetTEU >= 0, "verwendeteKapazitaetTEU >= 0");
            Check.Argument(verwendeteKapazitaetFEU >= 0, "verwendeteKapazitaetFEU >= 0");
            Check.Argument(verwendeteKapazitaetTEU + verwendeteKapazitaetFEU >= 0, "verwendeteKapazitaetTEU+verwendeteKapazitaetFEU >= 1");
            Check.Argument(saNr > 0, "saNr > 0");

            return transactionService.ExecuteTransactionalIfNoTransactionProvided(
                () => 
                {
                    FrachtfuehrerRahmenvertrag frv = this.frv_REPO.FindByFrvNr(frvNr);
                    if (frv == null)
                    {
                        throw new FrachtfuehrerRahmenvertragNichtGefundenException(frvNr);
                    }
                    return this.ubK_BL.BeaufrageTransport(frv, planStartzeit, planEndezeit, verwendeteKapazitaetTEU, verwendeteKapazitaetFEU, saNr); 
                });
        }

        public void SpeichereFrachtauftrag(ref FrachtauftragDTO faDTO)
        {
            Check.Argument(faDTO.FraNr >= 0, "faDTO.FraNr >= 0");
            Check.OperationCondition(!transactionService.IsTransactionActive, "Keine aktive Transaktion erlaubt.");

            Frachtauftrag fa = faDTO.ToEntity();
            transactionService.ExecuteTransactional(
                 () =>
                 {
                     this.fra_REPO.Add(fa);
                 });
        }

        #region IUnterbeauftargungServiceFuerBuchhaltung
        public void SchliesseFrachtauftragAb(int faufNr)
        {
            Check.Argument(faufNr >= 0, "faufNr >= 0");

            transactionService.ExecuteTransactional(
                () =>
                {
                    Frachtauftrag fauf = this.fra_REPO.FindByFaufNr(faufNr);
                    fauf.Status = FrachtauftragStatusTyp.Abgeschlossen;
                    this.fra_REPO.Add(fauf);
                });
        }
        #endregion
    }
}