using Common.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Util.Common.Interfaces;
using Util.MessagingServices.Implementations;
using Util.MessagingServices.Interfaces;

/*
Erstellen Sie das Nachbarsystem BankExternal als Konsolenanwendung, welches die vom HLS
verschickten Gutschriften aus der Queue liest und ausgibt. Legen Sie dazu ein neues Projekt
im Ordner „4 - External Systems“ an.
*/

namespace BankExternal
{
    internal class GutschriftDetail : DTOType<GutschriftDetail>
    {
        public int GutschriftNr { get; set; }
        public KontodatenType Kontodaten { get; set; }
        public WaehrungsType Betrag { get; set; }

        public override string ToString()
        {
            return "Gutschrift: Nr." + this.GutschriftNr + " Kto-Daten: " + this.Kontodaten + " Betrag: " + this.Betrag;
        }
    }

    internal class ZahlungseingangDetail : DTOType<ZahlungseingangDetail>
    {
        public int ZahlungsNr { get; set; }
        public decimal Zahlungsbetrag { get; set; }
        public int Kundenrechnung { get; set; }

        public override string ToString()
        {
            return "Zahlungseingang: Nr." + this.ZahlungsNr + " Betrag: " + this.Zahlungsbetrag + " Kundenrechnung: " + this.Kundenrechnung;
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var gutschrift = Task.Factory.StartNew(() => ReciveGutschriften());

            var zahlungseinang = Task.Factory.StartNew(() => SendZahlungseingaenge());

            Task.WaitAll(gutschrift, zahlungseinang); //Lässt Console geöffnet
        }

        private static void ReciveGutschriften()
        {
            IMessagingServices messagingManager = null;
            IQueueServices<GutschriftDetail> gutschriftDetailQueue = null;

            messagingManager = MessagingServicesFactory.CreateMessagingServices();
            gutschriftDetailQueue = messagingManager.CreateQueue<GutschriftDetail>("HLS.Queue.Gutschrift.Team3");

            Console.WriteLine("Warte auf Gutschriften in Queue '" + gutschriftDetailQueue.Queue + "'.");

            while (true)
            {
                GutschriftDetail gutschriftEmpfangen = gutschriftDetailQueue.ReceiveSync((o) =>
                {
                    return MessageAckBehavior.AcknowledgeMessage;
                });
                Console.Beep();
                Console.WriteLine("<== Gutschrift empfangen.");
            }
        }

        private static void SendZahlungseingaenge()
        {
            IMessagingServices messagingManager = null;
            IQueueServices<ZahlungseingangDetail> zahlungseingangDetailQueue = null;

            messagingManager = MessagingServicesFactory.CreateMessagingServices();
            zahlungseingangDetailQueue = messagingManager.CreateQueue<ZahlungseingangDetail>("HLS.Queue.Zahlungseingang.Team3");

            ////3890 = 10 * 389
            decimal wert = 3000;
            for (int i = 1; i < 3; i++)
            {
                ZahlungseingangDetail fakeZahlungseingangDetail = new ZahlungseingangDetail() { ZahlungsNr = i, Zahlungsbetrag = wert, Kundenrechnung = 1 };
                zahlungseingangDetailQueue.Send(fakeZahlungseingangDetail);
                Console.WriteLine("Folgender Zahlungseinang wurde in die Queue '" + zahlungseingangDetailQueue.Queue + "' gesendet: " + fakeZahlungseingangDetail.ToString());
                Console.Beep();
                wert = 890;
            }
        }
    }
}
