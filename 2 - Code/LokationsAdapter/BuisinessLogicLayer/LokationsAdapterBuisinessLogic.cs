using ApplicationCore.SendungKomponente.AccessLayer;
using ApplicationCore.SendungKomponente.DataAccessLayer;
using ApplicationCore.TransportnetzKomponente.DataAccessLayer;
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

namespace ApplicationCore.LokationsAdapter.BuisinessLogicLayer
{
    internal class SendungsverfolgungsereignisDetailDTO : DTOType<SendungsverfolgungsereignisDetailDTO>
    {
        public DateTime Zeitpunkt { get; set; }
        public SendungsverfolgungsereignisArtTyp Ereignisart { get; set; }
        public string Nachrichteninhalt { get; set; }
        public string Ort { get; set; }
        public int SNr { get; set; }
    }

    internal class LokationsAdapterBuisinessLogic
    {
        private ISendungServicesfürLokationsAdapter sendungServicesfürLokationsAdapter;
        private bool empfangeSendungsverfolgungsereignis;
        private IList<LokationDTO> lokationen;

        public LokationsAdapterBuisinessLogic(ISendungServicesfürLokationsAdapter sendungServicesfürLokationsAdapter, IList<LokationDTO> lokationen)
        {
            this.sendungServicesfürLokationsAdapter = sendungServicesfürLokationsAdapter;
            this.lokationen = lokationen;
        }

        private void EmpfangeSendungsverfolgungsereignisAusQueue()
        {
            IMessagingServices messagingManager = null;
            IQueueServices<SendungsverfolgungsereignisDetailDTO> sendungsverfolgungsereignisDetailQueue = null;

            System.Configuration.ConnectionStringSettings connectionSettings = System.Configuration.ConfigurationManager.ConnectionStrings["LokationExternal"];
            Contract.Assert(connectionSettings != null, "A LokationExternal connection setting needs to be defined in the App.config.");
            string sendungsverfolgungQueue = connectionSettings.ConnectionString;
            Contract.Assert(string.IsNullOrEmpty(sendungsverfolgungQueue) == false);

            messagingManager = MessagingServicesFactory.CreateMessagingServices();
            sendungsverfolgungsereignisDetailQueue = messagingManager.CreateQueue<SendungsverfolgungsereignisDetailDTO>(sendungsverfolgungQueue);

            Console.WriteLine("Warte auf Sendungsverfolgungsereignis in Queue '" + sendungsverfolgungsereignisDetailQueue.Queue + "'...");
            while (empfangeSendungsverfolgungsereignis)
            {
                SendungsverfolgungsereignisDetailDTO sendungsverfolgungsereignisDetailReceived = sendungsverfolgungsereignisDetailQueue.ReceiveSync((o) =>
                {
                    return MessageAckBehavior.AcknowledgeMessage;
                });

                Console.WriteLine("<== Sendungsverfolgungsereignis mit Nachricht '" + sendungsverfolgungsereignisDetailReceived.Nachrichteninhalt.ToString() + "' empfangen.");
                SendungsverfolgungsereignisDTO sveDTO = new SendungsverfolgungsereignisDTO()
                {
                    Ereignisart = sendungsverfolgungsereignisDetailReceived.Ereignisart,
                    Zeitpunkt = sendungsverfolgungsereignisDetailReceived.Zeitpunkt,
                    Nachrichteninhalt = sendungsverfolgungsereignisDetailReceived.Nachrichteninhalt,
                    SNr = sendungsverfolgungsereignisDetailReceived.SNr,
                };
                foreach (LokationDTO lokDTO in lokationen)
                {
                    if (lokDTO.Name == sendungsverfolgungsereignisDetailReceived.Ort)
                    {
                        sveDTO.Ort = lokDTO.LokNr;
                    }
                }
                this.sendungServicesfürLokationsAdapter.VerarbeiteSendungsverfolgungsereignis(ref sveDTO);
            }
        }

        public void StarteEmpfangVonSendungsverfolgungsereignis()
        {
            var tasks = Task.Factory.StartNew(() => EmpfangeSendungsverfolgungsereignisAusQueue());
            Task.WaitAll(tasks);
        }

        public void SetEmpfangeSendungsverfolgungsereignis(bool b)
        {
            this.empfangeSendungsverfolgungsereignis = b;
        }
    }
}
