using ApplicationCore.BuchhaltungKomponente.AccessLayer;
using ApplicationCore.BuchhaltungKomponente.DataAccessLayer;
using Common.DataTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util.Common.Interfaces;
using Util.MessagingServices.Implementations;
using Util.MessagingServices.Interfaces;

namespace ApplicationCore.BankAdapter.BusinessLogicLayer
{
    internal class GutschriftDetailDTO : DTOType<GutschriftDetailDTO>
    {
        public int GutschriftNr { get; set; }
        public KontodatenType Kontodaten { get; set; }
        public WaehrungsType Betrag { get; set; }
    }

    internal class ZahlungseingangDetailDTO : DTOType<ZahlungseingangDetailDTO>
    {
        public int ZahlungsNr { get; set; }
        public decimal Zahlungsbetrag { get; set; }
        public int Kundenrechnung { get; set; }
    }

    internal class BankAdapterBuisinessLogic
    {
        private IBuchhaltungsServicesFuerBank buchhaltungsServiceFuerBank;
        private bool empfange;

        internal BankAdapterBuisinessLogic()
        {
            this.empfange = true;
        }

        public void SendeGutschriftAnBank(GutschriftDTO gutschrift)
        {
            Contract.Requires(gutschrift != null);

            ////Console.Out.WriteLine("Folgende Gutschrift wurde an die Bank gesendet: ");
            ////Console.Out.WriteLine("Gutschrift Nummer: {0}", gutschrift.GutschriftNr);
            ////Console.Out.WriteLine("Gutschrift Kontodaten: {0}", gutschrift.Kontodaten);
            ////Console.Out.WriteLine("Gutschrift Betrag: {0}", gutschrift.Betrag);

            IMessagingServices messagingManager = null;
            IQueueServices<GutschriftDetailDTO> gutschriftDetailQueue = null;

            System.Configuration.ConnectionStringSettings connectionSettings = System.Configuration.ConfigurationManager.ConnectionStrings["BankExternal"];
            Contract.Assert(connectionSettings != null, "A BankExternal connection setting needs to be defined in the App.config.");
            string gutschriftQueue = connectionSettings.ConnectionString;
            Contract.Assert(string.IsNullOrEmpty(gutschriftQueue) == false);

            messagingManager = MessagingServicesFactory.CreateMessagingServices();
            gutschriftDetailQueue = messagingManager.CreateQueue<GutschriftDetailDTO>(gutschriftQueue);

            GutschriftDetailDTO gdDTO = new GutschriftDetailDTO()
            {
                GutschriftNr = gutschrift.GutschriftNr,
                Kontodaten = gutschrift.Kontodaten,
                Betrag = gutschrift.Betrag
            };
            gutschriftDetailQueue.Send(gdDTO);
            Console.WriteLine("==> Gutschrift wurde an Bank gesendet");
        }

        internal void EmpfangeZahlungseingaengenAusQueue()
        {
            IMessagingServices messagingManager = null;
            IQueueServices<ZahlungseingangDetailDTO> zahlungseingangDetailQueue = null;

            System.Configuration.ConnectionStringSettings connectionSettings = System.Configuration.ConfigurationManager.ConnectionStrings["Zahlungseingang"];
            Contract.Assert(connectionSettings != null, "A Zahlungseingang connection setting needs to be defined in the App.config.");
            string zahlungseingangQueue = connectionSettings.ConnectionString;
            Contract.Assert(string.IsNullOrEmpty(zahlungseingangQueue) == false);

            messagingManager = MessagingServicesFactory.CreateMessagingServices();
            zahlungseingangDetailQueue = messagingManager.CreateQueue<ZahlungseingangDetailDTO>(zahlungseingangQueue);

            while (empfange)
            {
                Console.WriteLine("Warte auf Zahlungseingänge in Queue '" + zahlungseingangDetailQueue.Queue + "'...");
                ZahlungseingangDetailDTO zahlungseinangDetailReceived = zahlungseingangDetailQueue.ReceiveSync((o) =>
                {
                    return MessageAckBehavior.AcknowledgeMessage;
                });

                Console.WriteLine("<== Zahlungseingang empfangen: " + zahlungseinangDetailReceived.ZahlungsNr.ToString());
                ZahlungseingangDTO zeDTO = new ZahlungseingangDTO()
                {
                    Zahlungsbetrag = new WaehrungsType(zahlungseinangDetailReceived.Zahlungsbetrag),
                    KrNr = zahlungseinangDetailReceived.Kundenrechnung,
                };
                buchhaltungsServiceFuerBank.VerarbeiteZahlungseingang(ref zeDTO);
            }
        }

        internal void EmpfangeZahlungseingaenge(bool b)
        {
            this.empfange = b;
        }

        internal void SetzeBuchhaltungServiceFuerBank(IBuchhaltungsServicesFuerBank buchhaltungsServicesFuerBank)
        {
            this.buchhaltungsServiceFuerBank = buchhaltungsServicesFuerBank;
        }
    }
}