using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Util.Common.Interfaces;

namespace ApplicationCore.UnterbeauftragungKomponente.DataAccessLayer
{
    public class Startzeit : ICanConvertToDTO<StartzeitDTO>
    {
        public virtual int SzNr { get; set; }
        public virtual DayOfWeek Wochentag { get; set; }
        public virtual int Uhrzeit { get; set; }

        public Startzeit()
        {
        }

        public virtual StartzeitDTO ToDTO()
        {
            StartzeitDTO szDTO = new StartzeitDTO();
            szDTO.SzNr = this.SzNr;
            szDTO.Wochentag = this.Wochentag;
            szDTO.Uhrzeit = this.Uhrzeit;
            return szDTO;
        }
    }

    public class FrachtfuehrerRahmenvertrag : ICanConvertToDTO<FrachtfuehrerRahmenvertragDTO>
    {
        public virtual int FrvNr { get; set; }
        public virtual DateTime GueltigkeitAb { get; set; }
        public virtual DateTime GueltigkeitBis { get; set; }
        public virtual TimeSpan Zeitvorgabe { get; set; }
        public virtual decimal KostenFix { get; set; }
        public virtual decimal KostenProTEU { get; set; }
        public virtual decimal KostenProFEU { get; set; }
        public virtual int KapazitaetTEU { get; set; }
        public virtual long FuerTransportAufTransportbeziehung { get; set; }
        public virtual IList<Startzeit> Abfahrtszeiten { get; set; }
        public virtual Frachtfuehrer Frachtfuehrer { get; set; }

        public FrachtfuehrerRahmenvertrag()
        {
            Abfahrtszeiten = new List<Startzeit>();
        }

        /// <summary>
        /// Berechnet Abfahrtszeiten als absolute Datumsangaben.
        /// Es werden nur Zeiten berechnet, die innerhalb des Gültigkeitszeitraumes des Vertrags liegen.
        /// </summary>
        /// <pre>von <= bis</pre>
        /// <post>Returnwert ist nicht null</post>
        public virtual List<DateTime> GetAbfahrtszeitenAbsolut(DateTime von, DateTime bis)
        {
            Contract.Requires(von <= bis);

            List<DateTime> abfahrtsZeitenAbsolut = new List<DateTime>();
            DateTime aktuellerTag = DateTime.Compare(von, GueltigkeitAb) < 0 ? GueltigkeitAb : von;
            DateTime spätesterTag = DateTime.Compare(bis, GueltigkeitBis) < 0 ? bis : GueltigkeitBis;
            while (aktuellerTag <= spätesterTag)
            {
                DayOfWeek dayOfWeek = System.Globalization.CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(aktuellerTag);
                foreach (Startzeit startzeit in Abfahrtszeiten)
                {
                    if (startzeit.Wochentag == dayOfWeek)
                    {
                        DateTime abfahrtZeitAbsolut = new DateTime(
                            aktuellerTag.Year, 
                            aktuellerTag.Month, 
                            aktuellerTag.Day,
                            startzeit.Uhrzeit, 
                            0, 
                            0);
                        abfahrtsZeitenAbsolut.Add(abfahrtZeitAbsolut);
                    }
                }
                aktuellerTag = aktuellerTag.AddDays(1);
            }

            Contract.Ensures(abfahrtsZeitenAbsolut != null);

            return abfahrtsZeitenAbsolut;
        }

        public virtual FrachtfuehrerRahmenvertragDTO ToDTO()
        {
            FrachtfuehrerRahmenvertragDTO frvDTO = new FrachtfuehrerRahmenvertragDTO();
            frvDTO.FrvNr = this.FrvNr;
            frvDTO.GueltigkeitAb = this.GueltigkeitAb;
            frvDTO.GueltigkeitBis = this.GueltigkeitBis;
            frvDTO.Zeitvorgabe = this.Zeitvorgabe;
            frvDTO.KostenFix = this.KostenFix;
            frvDTO.KostenProTEU = this.KostenProTEU;
            frvDTO.KostenProFEU = this.KostenProFEU;
            frvDTO.KapazitaetTEU = this.KapazitaetTEU;
            frvDTO.FuerTransportAufTransportbeziehung = this.FuerTransportAufTransportbeziehung;
            frvDTO.Frachtfuehrer = this.Frachtfuehrer.ToDTO();
            foreach (Startzeit sz in this.Abfahrtszeiten)
            {
                frvDTO.Abfahrtszeiten.Add(sz.ToDTO());
            }
            return frvDTO;
        }
    }

    internal class FrachtfuehrerRahmenvertragMap : ClassMap<FrachtfuehrerRahmenvertrag>
    {
        public FrachtfuehrerRahmenvertragMap()
        {
            this.Id(x => x.FrvNr);

            this.Map(x => x.FuerTransportAufTransportbeziehung);
            this.Map(x => x.GueltigkeitAb);
            this.Map(x => x.GueltigkeitBis);
            this.Map(x => x.KostenFix);
            this.Map(x => x.KostenProTEU);
            this.Map(x => x.KostenProFEU);
            this.Map(x => x.KapazitaetTEU);
            this.Map(x => x.Zeitvorgabe);
            this.HasMany(x => x.Abfahrtszeiten).Cascade.All();
            this.References(x => x.Frachtfuehrer);
        }
    }

    internal class StartzeitMap : ClassMap<Startzeit>
    {
        public StartzeitMap()
        {
            this.Id(x => x.SzNr);

            this.Map(x => x.Wochentag);
            this.Map(x => x.Uhrzeit);
        }
    }
}
