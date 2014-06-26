using ApplicationCore.AuftragKomponente.AccessLayer;
using ApplicationCore.BuchhaltungKomponente.AccessLayer;
using ApplicationCore.BuchhaltungKomponente.DataAccessLayer;
using ApplicationCore.GeschaeftspartnerKomponente.AccessLayer;
using ApplicationCore.TransportplanungKomponente.AccessLayer;
using ApplicationCore.UnterbeauftragungKomponente.AccessLayer;
using ApplicationCore.UnterbeauftragungKomponente.DataAccessLayer;
using Common.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Util.PersistenceServices.Implementations;
using Util.PersistenceServices.Interfaces;

namespace Tests.KomponentenTest.BuchhaltungKomponente
{
    [TestClass]
    public class KomponentenTest_BuchhaltungKomponente
    {
        private static IPersistenceServices persistenceService = null;
        private static ITransactionServices transactionService = null;
        private static IBuchhaltungServices buchhaltungsService = null;
        private static Mock<IUnterbeauftragungServicesFuerBuchhaltung> unterbeauftragungService = null;
        private static Mock<IBankServicesFuerBuchhaltung> bankService = null;
        private static Mock<ITransportplanServicesFuerBuchhaltung> transportplanServicesFuerBuchhaltung = null;
        private static Mock<IAuftragServicesFuerBuchhaltung> auftragServicesFuerBuchhaltung = null;
        private static Mock<IGeschaeftspartnerServices> geschaeftspartnerServices = null;

        /// <summary>
        /// Initialize the persistence
        /// </summary>
        /// <param name="testContext">Testcontext provided by framework</param>
        [ClassInitialize]
        public static void InitializePersistence(TestContext testContext)
        {
            log4net.Config.XmlConfigurator.Configure();

            PersistenceServicesFactory.CreateSimpleMySQLPersistenceService(out persistenceService, out transactionService);
            unterbeauftragungService = new Mock<IUnterbeauftragungServicesFuerBuchhaltung>();
            Mock<IPDFErzeugungsServicesFuerBuchhaltung> pDFErzeugungsServicesFuerBuchhaltung = new Mock<IPDFErzeugungsServicesFuerBuchhaltung>();
            bankService = new Mock<IBankServicesFuerBuchhaltung>();
            transportplanServicesFuerBuchhaltung = new Mock<ITransportplanServicesFuerBuchhaltung>();
            auftragServicesFuerBuchhaltung = new Mock<IAuftragServicesFuerBuchhaltung>();
            geschaeftspartnerServices = new Mock<IGeschaeftspartnerServices>();
            buchhaltungsService = new BuchhaltungKomponenteFacade(
                persistenceService, 
                transactionService, 
                unterbeauftragungService.Object, 
                bankService.Object, 
                transportplanServicesFuerBuchhaltung.Object,
                auftragServicesFuerBuchhaltung.Object,
                geschaeftspartnerServices.Object,
                pDFErzeugungsServicesFuerBuchhaltung.Object);
        }

        [TestMethod]
        public void TestCreateFrachtabrechnungSuccess()
        {
            FrachtauftragDTO faufDTO = new FrachtauftragDTO();
            buchhaltungsService.CreateFrachtabrechnung(ref faufDTO);
        }

        [TestMethod]
        public void TestPayFrachtabrechnungSuccess()
        {
            Frachtabrechnung fab = new Frachtabrechnung() { Rechnungsbetrag = new WaehrungsType(50), IstBestaetigt = true };
            FrachtabrechnungDTO fabDTO = fab.ToDTO();
            buchhaltungsService.PayFrachtabrechnung(ref fabDTO);
            fab = fabDTO.ToEntity();
            Assert.IsTrue(fab.Rechnungsbetrag == new WaehrungsType(50), "Rechnungsbetrag must be 50 in this case");
            Assert.IsTrue(fab.IstBestaetigt == true, "IstBestaetigt must be true in this case");
        }

        [TestMethod]
        public void TestDeleteFrachtabrechnungSuccess()
        {
            FrachtauftragDTO faufDTO = new FrachtauftragDTO();
            Frachtabrechnung fab = new Frachtabrechnung() { Rechnungsbetrag = new WaehrungsType(50), IstBestaetigt = true };
 
            FrachtabrechnungDTO fabDTO = fab.ToDTO();
            buchhaltungsService.CreateFrachtabrechnung(ref faufDTO);
            buchhaltungsService.DeleteFrachtabrechnung(ref fabDTO);
        }
    }
}