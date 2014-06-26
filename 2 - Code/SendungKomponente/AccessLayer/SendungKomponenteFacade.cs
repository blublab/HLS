using ApplicationCore.AuftragKomponente.AccessLayer;
using ApplicationCore.BuchhaltungKomponente.AccessLayer;
using ApplicationCore.SendungKomponente.DataAccessLayer;
using ApplicationCore.TransportplanungKomponente.AccessLayer;
using Common.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util.PersistenceServices.Interfaces;

namespace ApplicationCore.SendungKomponente.AccessLayer
{
    public class SendungKomponenteFacade : ISendungServices, ISendungServicesfürLokationsAdapter
    {
        private readonly SendungRepository se_REPO;
        private readonly ITransactionServices transactionService;
        private readonly ITransportplanungServicesFuerSendung transportplanungServicesFuerSendung;
        private readonly IAuftragServices auftragService;
        private readonly IBuchhaltungServicesFuerSendung buchhaltungServiceFuerSendung;

        public SendungKomponenteFacade(
            IPersistenceServices persistenceService,
            ITransactionServices transactionService,
            ITransportplanungServicesFuerSendung transportplanungServicesFuerSendung,
            IAuftragServices auftragService,
            IBuchhaltungServicesFuerSendung buchhaltungServicesFuerSendung)
        {
            Check.Argument(persistenceService != null, "persistenceService != null");
            Check.Argument(transactionService != null, "transactionService != null");
            Check.Argument(transportplanungServicesFuerSendung != null, "transportplanungServicesFuerSendung != null");

            this.transactionService = transactionService;
            this.se_REPO = new SendungRepository(persistenceService);
            this.transportplanungServicesFuerSendung = transportplanungServicesFuerSendung;
            this.auftragService = auftragService;
            this.buchhaltungServiceFuerSendung = buchhaltungServicesFuerSendung;
        }

        #region ISendungServices
        public void ErstelleSendung(int tpNr, int saNr)
        {
            Check.Argument(tpNr > 0, "tpNr muss größer als 0 sein.");
            Check.Argument(saNr > 0, "saNr muss größer als 0 sein.");

            Sendung s = new Sendung();
            s.TpNr = tpNr;
            s.SaNr = saNr;

            transactionService.ExecuteTransactional(
                        () =>
                        {
                            se_REPO.Save(s);
                        });

            transportplanungServicesFuerSendung.FühreTransportplanAus(s.TpNr);
        }
        #endregion

        #region ISendungServicesfürLokationsAdapter
        public void VerarbeiteSendungsverfolgungsereignis(ref SendungsverfolgungsereignisDTO sveDTO)
        {
            Check.Argument(sveDTO != null, "sveDTO != null.");

            Sendungsverfolgungsereignis sve = sveDTO.ToEntity();
            Sendung s = null;
            transactionService.ExecuteTransactional(
                () =>
                {
                    s = se_REPO.FindBySNr(sve.SNr);
                    s.SveLst.Add(sve);
                    se_REPO.Save(s);
                });

            long zL = auftragService.GibZielLokationFuerSendungsanfrage(s.SaNr);

            if (sve.Ort == zL && sve.Ereignisart == SendungsverfolgungsereignisArtTyp.ZielErreicht)
            {
                transportplanungServicesFuerSendung.UpdateTransportplanstatus(s.TpNr, TransportplanungKomponente.DataAccessLayer.TransportplanStatusTyp.Abgeschlossen);
                buchhaltungServiceFuerSendung.ErstelleKundenrechnung(s.TpNr, s.SaNr);
            }
        }
        #endregion
    }
}
