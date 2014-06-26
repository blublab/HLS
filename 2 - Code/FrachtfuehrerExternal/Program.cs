using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.DataTypes;
using Util.Common.Interfaces;
using Util.MessagingServices.Implementations;
using Util.MessagingServices.Interfaces;

namespace FrachtfuehrerExternal
{
    internal class FrachtauftragDetail : DTOType<FrachtauftragDetail>
    {
        public int FaNr { get; set; }
        public int FrfNr { get; set; }
        public int FrvNr { get; set; }
        public DateTime PlanStartzeit { get; set; }
        public DateTime PlanEndezeit { get; set; }
        public int VerwendeteKapazitaetTEU { get; set; }
        public int VerwendeteKapazitaetFEU { get; set; }

        public override string ToString()
        {
            return "Frachtauftrag: " + this.FaNr + " Frf: " + this.FrfNr + " Frv: " + this.FrvNr + " Start: " + this.PlanStartzeit + " Ende: " + this.PlanEndezeit + " TEU: " + this.VerwendeteKapazitaetTEU + " FEU: " + this.VerwendeteKapazitaetFEU;
        }
    }

    internal class FrachabrechnungDetail : DTOType<FrachabrechnungDetail>
    {
        public int RechnungsNr { get; set; }
        public bool IstBestaetigt { get; set; }
        public WaehrungsType Rechnungsbetrag { get; set; }
        public int Frachtauftrag { get; set; }

        public override string ToString()
        {
            return "Frachtabrechnung: " + this.RechnungsNr + " Frachtauftrag: " + this.Frachtauftrag + " Bestaetigt: " + this.IstBestaetigt + " Rechnungsbetrag: " + this.Rechnungsbetrag.ToString();
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var fill = Task.Factory.StartNew(() => SendFrachtabrechnung());

            var read = Task.Factory.StartNew(() => ReciveMessage());
            Task.WaitAll(fill, read); //Lässt Console geöffnet
        }

        private static void ReciveMessage()
        {
            IMessagingServices messagingManager = null;
            IQueueServices<FrachtauftragDetail> frachtauftragDetailQueue = null;

            messagingManager = MessagingServicesFactory.CreateMessagingServices();
            frachtauftragDetailQueue = messagingManager.CreateQueue<FrachtauftragDetail>("HLS.Queue.Frachtauftrag.Team3");

            Console.WriteLine("Warte auf Frachtaufträge in Queue '" + frachtauftragDetailQueue.Queue + "'.");

            while (true)
            {
                FrachtauftragDetail frachtauftragDetailReceived = frachtauftragDetailQueue.ReceiveSync((o) =>
                {
                    return MessageAckBehavior.AcknowledgeMessage;
                });
                Console.WriteLine("<== Frachtauftrag empfangen: " + frachtauftragDetailReceived.ToString());
            }
        }

        private static void SendFrachtabrechnung()
        {
            IMessagingServices messagingManager = null;
            IQueueServices<FrachabrechnungDetail> frachtabrechnungDetailQueue = null;

            messagingManager = MessagingServicesFactory.CreateMessagingServices();
            frachtabrechnungDetailQueue = messagingManager.CreateQueue<FrachabrechnungDetail>("HLS.Queue.Frachtabrechnung.Team3");
                        
            for (int i = 1; i < 4; i++)
                {
                    Thread.Sleep(500);
                    FrachabrechnungDetail fakeFrachtabrechnungDetail = new FrachabrechnungDetail() { Frachtauftrag = i, IstBestaetigt = true, Rechnungsbetrag = new WaehrungsType(i), RechnungsNr = i };
                    frachtabrechnungDetailQueue.Send(fakeFrachtabrechnungDetail);
                    Console.WriteLine("Folgende Frachtabrechnung wurde in die Queue '" + frachtabrechnungDetailQueue.Queue + "' gesendet: " + fakeFrachtabrechnungDetail.ToString());
                    Console.Beep();
                }
        }
    }
}
