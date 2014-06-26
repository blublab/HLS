using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading;
using Util.PersistenceServices.Implementations;
using Util.PersistenceServices.Interfaces;
using Util.TimeServices;

namespace Test.Integrationtest
{
    [TestClass]
    [DeploymentItem("Configurations/ConnectionStrings.config", "Configurations")]
    [DeploymentItem("NHibernate.ByteCode.Castle.dll")]
    [DeploymentItem("NHibernate.dll")]
    [DeploymentItem("NHibernate.Driver.MySqlDataDriver.dll")]
    [DeploymentItem("Mysql.Data.dll")]
    public class IntegrationsTest_Unterbeauftragungskomponente
    {
        private static IPersistenceServices persistenceService = null;
        private static ITransactionServices transactionService = null;
        
        [ClassInitialize]
        public static void InitializeTests(TestContext testContext)
        {
            log4net.Config.XmlConfigurator.Configure();

            PersistenceServicesFactory.CreateSimpleMySQLPersistenceService(out persistenceService, out transactionService);

            var timeServicesMock = new Mock<ITimeServices>();
        }

        [ClassCleanup]
        public static void CleanUpClass()
        {
        }

        [TestCleanup]
        public void CleanUpTest()
        {
        }
    }
}
