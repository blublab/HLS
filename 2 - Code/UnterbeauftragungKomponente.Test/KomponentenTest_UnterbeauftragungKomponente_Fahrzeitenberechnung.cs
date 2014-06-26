using ApplicationCore.UnterbeauftragungKomponente.DataAccessLayer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Tests.KomponentenTest.UnterbeauftragungKomponente
{
    [TestClass]
    public class KomponentenTest_UnterbeauftragungKomponente_Fahrzeitenberechnung
    {
        [ClassInitialize]
        public static void InitializeTests(TestContext testContext)
        {
        }

        [TestMethod]
        public void TestFRVFahrzeitenVollstaendigInGueltikeitszeitraumSuccess()
        {
            FrachtfuehrerRahmenvertrag frv = new FrachtfuehrerRahmenvertrag();
            frv.GueltigkeitAb = DateTime.Parse("29.07.2013"); // Montag
            frv.GueltigkeitBis = DateTime.Parse("04.08.2013"); // Sonntag
            frv.Abfahrtszeiten = new System.Collections.Generic.List<Startzeit>() 
            { 
                new Startzeit() { Wochentag = DayOfWeek.Monday, Uhrzeit = 8 },
                new Startzeit() { Wochentag = DayOfWeek.Wednesday, Uhrzeit = 9 },
                new Startzeit() { Wochentag = DayOfWeek.Wednesday, Uhrzeit = 12 },
                new Startzeit() { Wochentag = DayOfWeek.Friday, Uhrzeit = 6 }
            };

            List<DateTime> zeiten = frv.GetAbfahrtszeitenAbsolut(frv.GueltigkeitAb, frv.GueltigkeitBis);
            Assert.IsTrue(zeiten.Count == 4);
            Assert.IsTrue(zeiten.Contains(DateTime.Parse("29.07.2013 08:00:00")));
            Assert.IsTrue(zeiten.Contains(DateTime.Parse("31.07.2013 09:00:00")));
            Assert.IsTrue(zeiten.Contains(DateTime.Parse("31.07.2013 12:00:00")));
            Assert.IsTrue(zeiten.Contains(DateTime.Parse("02.08.2013 06:00:00")));
        }

        [TestMethod]
        public void TestFRVFahrzeitenTeilweiseInGueltigkeitsZeitraumSuccess()
        {
            FrachtfuehrerRahmenvertrag frv = new FrachtfuehrerRahmenvertrag();
            frv.GueltigkeitAb = DateTime.Parse("29.07.2013"); // Montag
            frv.GueltigkeitBis = DateTime.Parse("04.08.2013"); // Sonntag
            frv.Abfahrtszeiten = new System.Collections.Generic.List<Startzeit>() 
            { 
                new Startzeit() { Wochentag = DayOfWeek.Monday, Uhrzeit = 8 },
                new Startzeit() { Wochentag = DayOfWeek.Wednesday, Uhrzeit = 9 },
                new Startzeit() { Wochentag = DayOfWeek.Wednesday, Uhrzeit = 12 },
                new Startzeit() { Wochentag = DayOfWeek.Thursday, Uhrzeit = 6 },
                new Startzeit() { Wochentag = DayOfWeek.Friday, Uhrzeit = 11 },
                new Startzeit() { Wochentag = DayOfWeek.Saturday, Uhrzeit = 7 }
            };

            List<DateTime> zeiten = frv.GetAbfahrtszeitenAbsolut(DateTime.Parse("30.07.2013"), DateTime.Parse("02.08.2013"));
            Assert.IsTrue(zeiten.Count == 4);
            Assert.IsTrue(zeiten.Contains(DateTime.Parse("31.07.2013 09:00:00")));
            Assert.IsTrue(zeiten.Contains(DateTime.Parse("31.07.2013 12:00:00")));
            Assert.IsTrue(zeiten.Contains(DateTime.Parse("01.08.2013 06:00:00")));
            Assert.IsTrue(zeiten.Contains(DateTime.Parse("02.08.2013 11:00:00")));
        }

        [TestMethod]
        public void TestFRVFahrzeitenMehrereWochenSuccess()
        {
            FrachtfuehrerRahmenvertrag frv = new FrachtfuehrerRahmenvertrag();
            frv.GueltigkeitAb = DateTime.Parse("29.07.2013"); // Montag
            frv.GueltigkeitBis = DateTime.Parse("10.08.2013"); // Montag zwei Wochen spaeter
            frv.Abfahrtszeiten = new System.Collections.Generic.List<Startzeit>() 
            { 
                new Startzeit() { Wochentag = DayOfWeek.Monday, Uhrzeit = 8 },
                new Startzeit() { Wochentag = DayOfWeek.Saturday, Uhrzeit = 7 },
                new Startzeit() { Wochentag = DayOfWeek.Sunday, Uhrzeit = 11 }
            };

            List<DateTime> zeiten = frv.GetAbfahrtszeitenAbsolut(DateTime.Parse("01.07.2013"), DateTime.Parse("20.08.2013"));
            Assert.IsTrue(zeiten.Count == 5);
            Assert.IsTrue(zeiten.Contains(DateTime.Parse("29.07.2013 08:00:00")));
            Assert.IsTrue(zeiten.Contains(DateTime.Parse("05.08.2013 08:00:00")));
            Assert.IsTrue(zeiten.Contains(DateTime.Parse("03.08.2013 07:00:00")));
            Assert.IsTrue(zeiten.Contains(DateTime.Parse("10.08.2013 07:00:00")));
            Assert.IsTrue(zeiten.Contains(DateTime.Parse("04.08.2013 11:00:00")));
        }

        [TestMethod]
        public void TestFRVFahrzeitenAußerhalbGueltigkeitSuccess()
        {
            FrachtfuehrerRahmenvertrag frv = new FrachtfuehrerRahmenvertrag();
            frv.GueltigkeitAb = DateTime.Parse("29.07.2013"); // Montag
            frv.GueltigkeitBis = DateTime.Parse("04.08.2013"); // Sonntag
            frv.Abfahrtszeiten = new System.Collections.Generic.List<Startzeit>() 
            { 
                new Startzeit() { Wochentag = DayOfWeek.Monday, Uhrzeit = 8 },
                new Startzeit() { Wochentag = DayOfWeek.Wednesday, Uhrzeit = 9 },
                new Startzeit() { Wochentag = DayOfWeek.Wednesday, Uhrzeit = 12 },
                new Startzeit() { Wochentag = DayOfWeek.Friday, Uhrzeit = 6 }
            };

            List<DateTime> zeiten = frv.GetAbfahrtszeitenAbsolut(DateTime.Parse("05.08.2013"), DateTime.Parse("10.09.2013"));
            Assert.IsTrue(zeiten.Count == 0);
        }
    }
}
