using ApplicationCore.AuftragKomponente.AccessLayer;
using ApplicationCore.BankAdapter.AccessLayer;
using ApplicationCore.BankAdapter.BusinessLogicLayer;
using ApplicationCore.BuchhaltungKomponente.AccessLayer;
using ApplicationCore.BuchhaltungKomponente.DataAccessLayer;
using ApplicationCore.GeschaeftspartnerKomponente.AccessLayer;
using ApplicationCore.TransportplanungKomponente.AccessLayer;
using ApplicationCore.UnterbeauftragungKomponente.AccessLayer;
using ApplicationCore.UnterbeauftragungKomponente.DataAccessLayer;
using Common.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading;
using Util.Common.Interfaces;
using Util.MessagingServices.Implementations;
using Util.MessagingServices.Interfaces;
using Util.PersistenceServices.Implementations;
using Util.PersistenceServices.Interfaces;
using Util.TimeServices;

namespace Test.Integrationtest
{
    [TestClass]
    public class IntegrationsTest_Buchhaltungskomponente
    {
        private static IPersistenceServices persistenceService = null;
        private static ITransactionServices transactionService = null;

        private static IBuchhaltungServices buchhaltungService = null;
        private static IBankServicesFuerBuchhaltung bankServiceFuerBuchhaltung = null;
        private static Mock<IUnterbeauftragungServicesFuerBuchhaltung> unterbeauftragungServiceFuerBuchhaltung = null;

        private static FrachtauftragDTO fauf1DTO;
        private static FrachtauftragDTO fauf2DTO;
        private static FrachtauftragDTO fauf3DTO;

        private static Gutschrift g1;

        private static FrachtabrechnungDTO fab1DTO;
        private static FrachtabrechnungDTO fab2DTO;
        private static FrachtabrechnungDTO fab3DTO;

        [ClassInitialize]
        public static void InitializeTests(TestContext testContext)
        {
            log4net.Config.XmlConfigurator.Configure();

            PersistenceServicesFactory.CreateSimpleMySQLPersistenceService(out persistenceService, out transactionService);

            var timeServicesMock = new Mock<ITimeServices>();

            bankServiceFuerBuchhaltung = new BankAdapterFacade();
            unterbeauftragungServiceFuerBuchhaltung = new Mock<IUnterbeauftragungServicesFuerBuchhaltung>();
            buchhaltungService = new BuchhaltungKomponenteFacade(
            persistenceService,
            transactionService,
            unterbeauftragungServiceFuerBuchhaltung.Object,
            bankServiceFuerBuchhaltung,
            new Mock<ITransportplanServicesFuerBuchhaltung>().Object,
            new Mock<IAuftragServicesFuerBuchhaltung>().Object,
            new Mock<IGeschaeftspartnerServices>().Object,
            new Mock<IPDFErzeugungsServicesFuerBuchhaltung>().Object);

            fauf1DTO = new FrachtauftragDTO() { FraNr = 1, Dokument = null, FrachtfuehrerRahmenvertrag = new FrachtfuehrerRahmenvertragDTO(), PlanEndezeit = new DateTime(), PlanStartzeit = new DateTime(), Status = FrachtauftragStatusTyp.NichtAbgeschlossen, VerwendeteKapazitaetFEU = 5, VerwendeteKapazitaetTEU = 10 };
            fauf2DTO = new FrachtauftragDTO() { FraNr = 2, Dokument = null, FrachtfuehrerRahmenvertrag = new FrachtfuehrerRahmenvertragDTO(), PlanEndezeit = new DateTime(), PlanStartzeit = new DateTime(), Status = FrachtauftragStatusTyp.NichtAbgeschlossen, VerwendeteKapazitaetFEU = 5, VerwendeteKapazitaetTEU = 10 };
            fauf3DTO = new FrachtauftragDTO() { FraNr = 3, Dokument = null, FrachtfuehrerRahmenvertrag = new FrachtfuehrerRahmenvertragDTO(), PlanEndezeit = new DateTime(), PlanStartzeit = new DateTime(), Status = FrachtauftragStatusTyp.NichtAbgeschlossen, VerwendeteKapazitaetFEU = 5, VerwendeteKapazitaetTEU = 10 };

            g1 = new Gutschrift() { GutschriftNr = 1, Betrag = new WaehrungsType(3), Kontodaten = new KontodatenType("DE00210501700012345678", "RZTIAT22263") };

            fab1DTO = new FrachtabrechnungDTO() { Gutschrift = g1, Frachtauftrag = fauf1DTO.FraNr, IstBestaetigt = true, Rechnungsbetrag = new WaehrungsType(30), RechnungsNr = 1 };
            fab2DTO = new FrachtabrechnungDTO() { Gutschrift = new Gutschrift(), Frachtauftrag = fauf2DTO.FraNr, IstBestaetigt = true, Rechnungsbetrag = new WaehrungsType(40), RechnungsNr = 2 };
            fab3DTO = new FrachtabrechnungDTO() { Gutschrift = new Gutschrift(), Frachtauftrag = fauf3DTO.FraNr, IstBestaetigt = true, Rechnungsbetrag = new WaehrungsType(50), RechnungsNr = 3 };
        }

        [ClassCleanup]
        public static void CleanUpClass()
        {
        }

        [TestMethod, TestCategory("IntegrationsTest")]
        public void TestErstelleFrachtabrechnungUndBezhaleSieSuccess()
        {
            buchhaltungService.CreateFrachtabrechnung(ref fauf1DTO);
            buchhaltungService.PayFrachtabrechnung(ref fab1DTO);
        }

        internal class GutschriftDetailDTO : DTOType<GutschriftDetailDTO>
        {
            public int GutschriftNr { get; set; }
            public KontodatenType Kontodaten { get; set; }
            public WaehrungsType Betrag { get; set; }
        }

        [TestMethod, TestCategory("IntegrationsTest")]
        public void TestErstelleFrachtabrechnungUndBezhaleSieUndUeberpruefeGutschriftSuccess()
        {
            buchhaltungService.CreateFrachtabrechnung(ref fauf1DTO);

            IMessagingServices messagingManager = null;
            IQueueServices<GutschriftDTO> gutschriftQueue = null;

            messagingManager = MessagingServicesFactory.CreateMessagingServices();
            gutschriftQueue = messagingManager.CreateQueue<GutschriftDTO>("HLS.Queue.Gutschrift.Team3");
            gutschriftQueue.Purge();

            buchhaltungService.PayFrachtabrechnung(ref fab1DTO);

            GutschriftDTO gutschriftEmpfangen = gutschriftQueue.ReceiveSync((o) =>
                {
                    return MessageAckBehavior.AcknowledgeMessage;
                });

            Gutschrift g2 = gutschriftEmpfangen.ToEntity();
            ////Assert.IsTrue(g1.Equals(g2)); //TODO: equals bzw ==
        }

        [TestCleanup]
        public void CleanUpTest()
        {
        }
    }
}
