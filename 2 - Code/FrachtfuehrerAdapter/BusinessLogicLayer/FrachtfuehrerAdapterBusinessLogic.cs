using ApplicationCore.BuchhaltungKomponente.AccessLayer;
using ApplicationCore.BuchhaltungKomponente.DataAccessLayer;
using ApplicationCore.UnterbeauftragungKomponente.DataAccessLayer;
using Common.DataTypes;
using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Util.Common.Interfaces;
using Util.MessagingServices.Implementations;
using Util.MessagingServices.Interfaces;

namespace ApplicationCore.FrachtfuehrerAdapter.BusinessLogicLayer
{
    internal class FrachtauftragDetailDTO : DTOType<FrachtauftragDetailDTO>
    {
        public int FaNr { get; set; }
        public int FrfNr { get; set; }
        public int FrvNr { get; set; }
        public DateTime PlanStartzeit { get; set; }
        public DateTime PlanEndezeit { get; set; }
        public int VerwendeteKapazitaetTEU { get; set; }
        public int VerwendeteKapazitaetFEU { get; set; }
    }

    internal class FrachtabrechnungDetailDTO : DTOType<FrachtabrechnungDetailDTO>
    {
        public int RechnungsNr { get; set; }
        public bool IstBestaetigt { get; set; }
        public WaehrungsType Rechnungsbetrag { get; set; }
        public int Frachtauftrag { get; set; }
    }

    internal class FrachtfuehrerAdapterBusinessLogic
    {
        private IBuchhaltungServices buchhaltungServices = null; 

        public FrachtfuehrerAdapterBusinessLogic(ref IBuchhaltungServices buchhaltungServices)
        {
            this.buchhaltungServices = buchhaltungServices;
        }

        public void SendeFrachtauftragAnFrachtfuehrer(FrachtauftragDTO fraDTO)
        {
            Contract.Requires(fraDTO != null);

            IMessagingServices messagingManager = null;
            IQueueServices<FrachtauftragDetailDTO> frachtauftragDetailQueue = null;

            System.Configuration.ConnectionStringSettings connectionSettings = System.Configuration.ConfigurationManager.ConnectionStrings["FrachtfuehrerExternalFrachtauftrag"];
            Contract.Assert(connectionSettings != null, "A FrachtfuehrerExternal connection setting needs to be defined in the App.config.");
            string frachtfuehrerQueue = connectionSettings.ConnectionString;
            Contract.Assert(string.IsNullOrEmpty(frachtfuehrerQueue) == false);

            messagingManager = MessagingServicesFactory.CreateMessagingServices();
            frachtauftragDetailQueue = messagingManager.CreateQueue<FrachtauftragDetailDTO>(frachtfuehrerQueue);
            FrachtauftragDetailDTO frachtauftragDetailSent = new FrachtauftragDetailDTO() 
            { 
                FaNr = fraDTO.FraNr, 
                FrfNr = fraDTO.FrachtfuehrerRahmenvertrag.Frachtfuehrer.FrfNr, 
                FrvNr = fraDTO.FrachtfuehrerRahmenvertrag.FrvNr,
                PlanStartzeit = fraDTO.PlanStartzeit, 
                PlanEndezeit = fraDTO.PlanEndezeit, 
                VerwendeteKapazitaetTEU = fraDTO.VerwendeteKapazitaetTEU, 
                VerwendeteKapazitaetFEU = fraDTO.VerwendeteKapazitaetFEU 
            };
            frachtauftragDetailQueue.Send(frachtauftragDetailSent);
        }

        private void EmpfangeFrachtabrechnungenAusQueue()
        {
            IMessagingServices messagingManager = null;
            IQueueServices<FrachtabrechnungDetailDTO> frachtabrechnungDetailQueue = null;

            System.Configuration.ConnectionStringSettings connectionSettings = System.Configuration.ConfigurationManager.ConnectionStrings["FrachtfuehrerExternalFrachtauftrag"];
            Contract.Assert(connectionSettings != null, "A FrachtfuehrerExternal connection setting needs to be defined in the App.config.");
            string frachtfuehrerQueue = connectionSettings.ConnectionString;
            Contract.Assert(string.IsNullOrEmpty(frachtfuehrerQueue) == false);

            messagingManager = MessagingServicesFactory.CreateMessagingServices();
            frachtabrechnungDetailQueue = messagingManager.CreateQueue<FrachtabrechnungDetailDTO>(frachtfuehrerQueue);

            while (true)
            {
                Console.WriteLine("Warte auf Frachtabrechnung in Queue '" + frachtabrechnungDetailQueue.Queue + "'...");
                FrachtabrechnungDetailDTO frachtabrechnungDetailReceived = frachtabrechnungDetailQueue.ReceiveSync((o) =>
                {
                    return MessageAckBehavior.AcknowledgeMessage;
                });
                
                Console.WriteLine("<== Frachtabrechnung empfangen: " + frachtabrechnungDetailReceived.RechnungsNr.ToString());
                FrachtabrechnungDTO fabDTO = new FrachtabrechnungDTO()
                {
                    Frachtauftrag = frachtabrechnungDetailReceived.Frachtauftrag,
                    IstBestaetigt = frachtabrechnungDetailReceived.IstBestaetigt,
                    Rechnungsbetrag = frachtabrechnungDetailReceived.Rechnungsbetrag,
                };
                this.buchhaltungServices.PayFrachtabrechnung(ref fabDTO);
            }
        }
        
        public void StarteEmpfangvonFrachabrechnungen()
        {
            var tasks = Task.Factory.StartNew(() => EmpfangeFrachtabrechnungenAusQueue());
            Task.WaitAll(tasks);
        }
    }
}
