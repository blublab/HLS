using ApplicationCore.GeschaeftspartnerKomponente.DataAccessLayer;
using Common.Implementations;
using System;
using System.Collections.Generic;
using Util.PersistenceServices.Interfaces;

namespace ApplicationCore.GeschaeftspartnerKomponente.AccessLayer
{
    public class GeschaeftspartnerKomponenteFacade : IGeschaeftspartnerServices, IGeschaeftspartnerServicesFuerPDFErzeugung
    {
        private readonly GeschaeftspartnerRepository gp_REPO;
        private readonly ITransactionServices transactionService;

        public GeschaeftspartnerKomponenteFacade(IPersistenceServices persistenceService, ITransactionServices transactionService)
        {
            Check.Argument(persistenceService != null, "persistenceService != null");
            Check.Argument(transactionService != null, "transactionService != null");

            this.gp_REPO = new GeschaeftspartnerRepository(persistenceService);
            this.transactionService = transactionService;
        }

        public void CreateGeschaeftspartner(ref GeschaeftspartnerDTO gpDTO)
        {
            Check.Argument(gpDTO != null, "gpDTO != null");
            Check.Argument(gpDTO.GpNr == 0, "gpDTO.Id == 0");
            Check.OperationCondition(!transactionService.IsTransactionActive, "Keine aktive Transaktion erlaubt.");

            Geschaeftspartner gp = gpDTO.ToEntity();
            transactionService.ExecuteTransactional(
                () => 
                {
                    this.gp_REPO.Save(gp);
                });
            gpDTO = this.FindGeschaeftspartner(gp.GpNr);
        }

        public void UpdateGeschaeftspartner(ref GeschaeftspartnerDTO gpDTO)
        {
            Check.Argument(gpDTO != null, "gpDTO != null");
            Check.Argument(gpDTO.GpNr > 0, "gpDTO.Id > 0");
            Check.OperationCondition(!transactionService.IsTransactionActive, "Keine aktive Transaktion erlaubt.");
            int gpNr = gpDTO.GpNr;
            transactionService.ExecuteTransactional(() =>
            {
                if (this.gp_REPO.FindByGpNr(gpNr) == null)
                {
                    throw new GeschaeftspartnerNichtGefundenException(gpNr);
                }
            });

            Geschaeftspartner gp = gpDTO.ToEntity();
            transactionService.ExecuteTransactional(() =>
                {
                    this.gp_REPO.Save(gp);
                });
            gpDTO = this.FindGeschaeftspartner(gp.GpNr);
        }

        public GeschaeftspartnerDTO FindGeschaeftspartner(int gpNr)
        {
            Check.Argument(gpNr > 0, "gpNr > 0");

            Geschaeftspartner gp = null;
            transactionService.ExecuteTransactionalIfNoTransactionProvided(
                () =>
                {
                    gp = this.gp_REPO.FindByGpNr(gpNr);
                });

            if (gp == null)
            {
                return null;
            }

            return gp.ToDTO();
        }

        public IList<GeschaeftspartnerDTO> SelectGeschaeftspartner()
        {
            IList<GeschaeftspartnerDTO> gpDTOList = new List<GeschaeftspartnerDTO>();
            IList<Geschaeftspartner> gpList = new List<Geschaeftspartner>();
            transactionService.ExecuteTransactionalIfNoTransactionProvided(
            () =>
            {
                gpList = this.gp_REPO.Select();
            });

            foreach (Geschaeftspartner gp  in gpList)
            {
                gpDTOList.Add(gp.ToDTO());
            }

            return gpDTOList;
        }

        #region IGeschaeftspartnerServicesFuerPDFErzeugung
        public AdresseDTO FindeAdresseZuID(int id)
        {
            Check.Argument(id > 0, "id > 0");

            return transactionService.ExecuteTransactional(() =>
            {
                return gp_REPO.FindAdrById(id).ToDTO();
            });
        }
        #endregion
    }
}
