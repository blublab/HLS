using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Util.Common.Interfaces;

namespace ApplicationCore.UnterbeauftragungKomponente.DataAccessLayer
{
    public class StartzeitDTO : DTOType<StartzeitDTO>, ICanConvertToEntity<Startzeit>
    {
        public virtual int SzNr { get; set; }
        public virtual DayOfWeek Wochentag { get; set; }
        public virtual int Uhrzeit { get; set; }

        public StartzeitDTO()
        {
        }

        public virtual Startzeit ToEntity()
        {
            Startzeit sz = new Startzeit();
            sz.SzNr = this.SzNr;
            sz.Wochentag = this.Wochentag;
            sz.Uhrzeit = this.Uhrzeit;
            return sz;
        }
    }

    public class FrachtfuehrerRahmenvertragDTO : DTOType<FrachtfuehrerRahmenvertragDTO>, ICanConvertToEntity<FrachtfuehrerRahmenvertrag>
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
        public virtual IList<StartzeitDTO> Abfahrtszeiten { get; set; }
        public virtual FrachtfuehrerDTO Frachtfuehrer { get; set; }

        public FrachtfuehrerRahmenvertragDTO()
        {
            Abfahrtszeiten = new List<StartzeitDTO>();
        }

        public virtual FrachtfuehrerRahmenvertrag ToEntity()
        {
            FrachtfuehrerRahmenvertrag frv = new FrachtfuehrerRahmenvertrag();
            frv.FrvNr = this.FrvNr;
            frv.GueltigkeitAb = this.GueltigkeitAb;
            frv.GueltigkeitBis = this.GueltigkeitBis;
            frv.Zeitvorgabe = this.Zeitvorgabe;
            frv.KostenFix = this.KostenFix;
            frv.KostenProTEU = this.KostenProTEU;
            frv.KostenProFEU = this.KostenProFEU;
            frv.KapazitaetTEU = this.KapazitaetTEU;
            frv.FuerTransportAufTransportbeziehung = this.FuerTransportAufTransportbeziehung;
            frv.Frachtfuehrer = this.Frachtfuehrer.ToEntity();
            foreach (StartzeitDTO szDTO in this.Abfahrtszeiten)
            {
                frv.Abfahrtszeiten.Add(szDTO.ToEntity());
            }
            return frv;
        }
    }
}
