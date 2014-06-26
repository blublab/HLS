using Common.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Tests.KomponentenTest.Common
{
    [TestClass]
    public class KomponentenTest_Common_DataTypes
    {
        [TestMethod]
        public void TestEMailTypSuccess()
        {
            Assert.IsTrue(EMailType.IsValid("stefan.sarstedt@haw-hamburg.de"));
        }

        [TestMethod]
        public void TestEMailTypKonstruktorSuccess()
        {
            EMailType emailType = new EMailType("stefan.sarstedt@haw-hamburg.de");
        }

        [TestMethod]
        public void TestEMailTypFail()
        {
            Assert.IsFalse(EMailType.IsValid("stefan.sarstedthaw-hamburg.de"));
            Assert.IsFalse(EMailType.IsValid("stefan.sarstedt@haw-hamburg."));
            Assert.IsFalse(EMailType.IsValid("stefan.sarstedt@haw-hamburg"));
            Assert.IsFalse(EMailType.IsValid("@haw-hamburg.de"));
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestEMailTypKonstruktorFail()
        {
            EMailType emailType = new EMailType("@haw-hamburg.de");
        }
    }
}
