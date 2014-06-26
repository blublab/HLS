using ApplicationCore.AuftragKomponente.AccessLayer;
using ApplicationCore.AuftragKomponente.DataAccessLayer;
using ApplicationCore.BuchhaltungKomponente.DataAccessLayer;
using ApplicationCore.GeschaeftspartnerKomponente.AccessLayer;
using ApplicationCore.GeschaeftspartnerKomponente.DataAccessLayer;
using ApplicationCore.TransportplanungKomponente.AccessLayer;
using ApplicationCore.TransportplanungKomponente.DataAccessLayer;
using ApplicationCore.UnterbeauftragungKomponente.AccessLayer;
using ApplicationCore.UnterbeauftragungKomponente.DataAccessLayer;
using Common.DataTypes;
using Common.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util.PersistenceServices.Interfaces;

namespace ApplicationCore.BuchhaltungKomponente.AccessLayer
{
    public class BuchhaltungKomponenteFacade : IBuchhaltungServices, IBuchhaltungServicesFuerSendung, IBuchhaltungsServicesFuerBank
    {
        private readonly BuchhaltungRepository bh_REPO;
        private readonly ITransactionServices transactionService;
        private readonly IUnterbeauftragungServicesFuerBuchhaltung unterbeauftragungService;
        private readonly IBankServicesFuerBuchhaltung bankService;
        private readonly ITransportplanServicesFuerBuchhaltung transportplanServiceFuerBuchhaltung;
        private readonly IAuftragServicesFuerBuchhaltung auftragServiceFuerBuchhaltung;
        private readonly IGeschaeftspartnerServices geschaeftspartnerService;
        private readonly IPDFErzeugungsServicesFuerBuchhaltung pDFErzeugungsServiceFuerBuchhaltung;

        public BuchhaltungKomponenteFacade(
            IPersistenceServices persistenceService,
            ITransactionServices transactionService,
            IUnterbeauftragungServicesFuerBuchhaltung unterbeauftragungService,
            IBankServicesFuerBuchhaltung bankService,
            ITransportplanServicesFuerBuchhaltung transportplanServicesFuerBuchhaltung,
            IAuftragServicesFuerBuchhaltung auftragServicesFuerBuchhaltung,
            IGeschaeftspartnerServices geschaeftspartnerServices,
            IPDFErzeugungsServicesFuerBuchhaltung pDFErzeugungsServicesFuerBuchhaltung)
        {
            Check.Argument(persistenceService != null, "persistenceService != null");
            Check.Argument(transactionService != null, "transactionService != null");
            Check.Argument(unterbeauftragungService != null, "unterbeauftragungService != null");
            Check.Argument(transportplanServicesFuerBuchhaltung != null, "transportplanServicesFuerBuchhaltung != null");
            Check.Argument(bankService != null, "bankService != null");
            Check.Argument(auftragServicesFuerBuchhaltung != null, "auftragServicesFuerBuchhaltung != null");
            Check.Argument(geschaeftspartnerServices != null, "geschaeftspartnerServices != null");
            Check.Argument(pDFErzeugungsServicesFuerBuchhaltung != null, "PDFErzeugungsServicesFuerBuchhaltung != null");

            this.bh_REPO = new BuchhaltungRepository(persistenceService);
            this.transactionService = transactionService;
            this.unterbeauftragungService = unterbeauftragungService;
            this.bankService = bankService;
            this.transportplanServiceFuerBuchhaltung = transportplanServicesFuerBuchhaltung;
            this.auftragServiceFuerBuchhaltung = auftragServicesFuerBuchhaltung;
            this.geschaeftspartnerService = geschaeftspartnerServices;
            this.pDFErzeugungsServiceFuerBuchhaltung = pDFErzeugungsServicesFuerBuchhaltung;
        }

        #region IBuchhaltungService
        public void CreateFrachtabrechnung(ref FrachtauftragDTO faufDTO)
        {
            Check.Argument(faufDTO != null, "faufDTO != null");
            Check.OperationCondition(!transactionService.IsTransactionActive, "Keine aktive Transaktion erlaubt.");

            Frachtabrechnung fab = new Frachtabrechnung();
            fab.Frachtauftrag = faufDTO.FraNr;
            transactionService.ExecuteTransactional(
                 () =>
                 {
                     this.bh_REPO.Save(fab);
                 });
        }

        public void PayFrachtabrechnung(ref FrachtabrechnungDTO fabDTO)
        {
            Check.Argument(fabDTO != null, "fabDTO != null");
            Check.OperationCondition(!transactionService.IsTransactionActive, "Keine aktive Transaktion erlaubt.");

            Frachtabrechnung fab = fabDTO.ToEntity();
            int faufNr = fab.Frachtauftrag;
            unterbeauftragungService.SchliesseFrachtauftragAb(faufNr);
            Gutschrift gutschrift = new Gutschrift();
            fab.Gutschrift = gutschrift;
            GutschriftDTO gutschriftDTO = gutschrift.ToDTO();
            bankService.SendeGutschriftAnBank(gutschriftDTO);
            transactionService.ExecuteTransactional(
                 () =>
                 {
                     this.bh_REPO.Save(fab);
                 });
        }

        public void DeleteFrachtabrechnung(ref FrachtabrechnungDTO fabDTO)
        {
            Frachtabrechnung fab = fabDTO.ToEntity();
            transactionService.ExecuteTransactional(
                () =>
                {
                    this.bh_REPO.DeleteFrachtabrechnung(fab);
                });
        }
        #endregion

        #region IBuchhaltungServicesFuerSendung
        public void ErstelleKundenrechnung(int tpNr, int saNr)
        {
            Check.Argument(tpNr > 0, "TpNr > 0");
            Check.Argument(saNr > 0, "saNr > 0");

            Check.OperationCondition(!transactionService.IsTransactionActive, "Keine aktive Transaktion erlaubt.");

            Kundenrechnung kr = new Kundenrechnung();
            kr.RechnungBezahlt = false;
            TransportplanDTO tpDTO = transportplanServiceFuerBuchhaltung.FindeTransportplanUeberTpNr(tpNr);
            kr.Sendungsanfrage = saNr;
            IList<TransportplanSchrittDTO> tpSchritte = tpDTO.TransportplanSchritte;

            decimal kostenSumme = 0;
            foreach (TransportplanSchrittDTO tpsDTO in tpSchritte)
            {
                kostenSumme += tpsDTO.Kosten;
            }

            kr.Rechnungsbetrag = new WaehrungsType(kostenSumme);
            kr.Sendungsanfrage = saNr;
            SendungsanfrageDTO saDTO = auftragServiceFuerBuchhaltung.FindeSendungsanfrageUeberSaNr(saNr);
            GeschaeftspartnerDTO gpDTO = geschaeftspartnerService.FindGeschaeftspartner(saDTO.Auftraggeber);
            kr.Rechnungsadresse = gpDTO.Adressen.First<AdresseDTO>().Id;

            transactionService.ExecuteTransactional(
                () =>
                {
                    bh_REPO.SpeichereKundenrechnung(kr);
                });
            KundenrechnungDTO krDTO = kr.ToDTO();
            pDFErzeugungsServiceFuerBuchhaltung.ErstelleKundenrechnungPDF(ref krDTO, tpSchritte);
        }
        #endregion

        #region IBuchhaltungsServicesFuerBank
        public void VerarbeiteZahlungseingang(ref ZahlungseingangDTO zeDTO)
        {
            Kundenrechnung kr = null;

            Zahlungseingang ze = zeDTO.ToEntity();
            transactionService.ExecuteTransactional(
                () =>
                {
                    bh_REPO.SpeichereZahlungseingang(ze);
                });
            transactionService.ExecuteTransactional(
                () =>
                {
                    kr = bh_REPO.GetKundenrechnungById(ze.KrNr);
                });
            kr.Rechnungsbetrag -= ze.Zahlungsbetrag;
            if (kr.Rechnungsbetrag.Wert <= 0)
            {
                kr.RechnungBezahlt = true;
                auftragServiceFuerBuchhaltung.SchliesseSendungsanfrageAb(kr.Sendungsanfrage);
            }
            transactionService.ExecuteTransactional(
                () =>
                {
                    bh_REPO.SpeichereKundenrechnung(kr);
                });
        }
        #endregion
    }
}
