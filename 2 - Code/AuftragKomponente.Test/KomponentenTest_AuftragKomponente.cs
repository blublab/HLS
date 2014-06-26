using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Tests.AuftragKomponente.Test
{
    [TestClass]
    public class KomponentenTest_AuftragKomponente
    {
        [ClassInitialize]
        public static void InitializeBla(TestContext testContext)
        {
        }

        [TestMethod]
        public void TestBlaSuccess()
        {
            Assert.IsTrue(true == true);
        }
    }
}
