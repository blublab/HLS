using ApplicationCore.SendungKomponente.DataAccessLayer;
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

namespace LokationExternal
{
    internal class SendungsverfolgungsereignisDetail : DTOType<SendungsverfolgungsereignisDetail>
    {
        public DateTime Zeitpunkt { get; set; }
        public SendungsverfolgungsereignisArtTyp Ereignisart { get; set; }
        public string Nachrichteninhalt { get; set; }
        public string Ort { get; set; }
        public int SNr { get; set; }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var fill = Task.Factory.StartNew(() => SendSendungsverfolgungsereignis());

            Task.WaitAll(fill); //Lässt Console geöffnet
        }

        private static void SendSendungsverfolgungsereignis()
        {
            IMessagingServices messagingManager = null;
            IQueueServices<SendungsverfolgungsereignisDetail> sendungsverfolgungsereignisDetailQueue = null;

            messagingManager = MessagingServicesFactory.CreateMessagingServices();
            sendungsverfolgungsereignisDetailQueue = messagingManager.CreateQueue<SendungsverfolgungsereignisDetail>("HLS.Queue.Sendungsverfolgung.Team3");

            SendungsverfolgungsereignisDetail sevDetail1 = new SendungsverfolgungsereignisDetail() { Nachrichteninhalt = "Nachricht: xy", Ort = "Hamburg", Zeitpunkt = DateTime.Now, Ereignisart = SendungsverfolgungsereignisArtTyp.Eingang, SNr = 1 };
            sendungsverfolgungsereignisDetailQueue.Send(sevDetail1);
            Console.WriteLine("Folgendes Sendungsverfolgungsereignis wurde in die Queue '" + sendungsverfolgungsereignisDetailQueue.Queue + "' gesendet: " + sevDetail1.Nachrichteninhalt.ToString());
            Console.WriteLine("");
            Console.Beep();

            SendungsverfolgungsereignisDetail sevDetail2 = new SendungsverfolgungsereignisDetail() { Nachrichteninhalt = "Nachricht: xy", Ort = "Hamburg", Zeitpunkt = DateTime.Now, Ereignisart = SendungsverfolgungsereignisArtTyp.Ausgang, SNr = 1 };
            sendungsverfolgungsereignisDetailQueue.Send(sevDetail2);
            Console.WriteLine("Folgendes Sendungsverfolgungsereignis wurde in die Queue '" + sendungsverfolgungsereignisDetailQueue.Queue + "' gesendet: " + sevDetail2.Nachrichteninhalt.ToString());
            Console.WriteLine("");
            Console.Beep();

            SendungsverfolgungsereignisDetail sevDetail3 = new SendungsverfolgungsereignisDetail() { Nachrichteninhalt = "Nachricht: xy", Ort = "Shanghai", Zeitpunkt = DateTime.Now, Ereignisart = SendungsverfolgungsereignisArtTyp.ZielErreicht, SNr = 1 };
            sendungsverfolgungsereignisDetailQueue.Send(sevDetail3);
            Console.WriteLine("Folgendes Sendungsverfolgungsereignis wurde in die Queue '" + sendungsverfolgungsereignisDetailQueue.Queue + "' gesendet: " + sevDetail3.Nachrichteninhalt.ToString());
            Console.WriteLine("");
            Console.Beep();
        }
    }
}
