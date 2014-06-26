using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util.Common.Interfaces;

namespace ApplicationCore.TransportplanungKomponente.DataAccessLayer
{
    public class TransportplanDTO : DTOType<TransportplanDTO>
    {
        public virtual int TpNr { get; set; }
        public virtual int SaNr { get; set; }
        public virtual TransportplanStatusTyp Status { get; set; }
        public virtual List<TransportplanMeldungTyp> Meldungen { get; set; }
        public virtual IList<TransportplanSchrittDTO> TransportplanSchritte { get; set; }
        public virtual IList<FrachteinheitDTO> Frachteinheiten { get; set; }
        public virtual decimal Kosten { get; set; }
        public virtual TimeSpan Dauer { get; set; }
        public virtual DateTime AbholungAm { get; set; }
        public virtual DateTime LieferungAm { get; set; }

        public TransportplanDTO()
        {
            this.TransportplanSchritte = new List<TransportplanSchrittDTO>();
            this.Frachteinheiten = new List<FrachteinheitDTO>();
            this.Meldungen = new List<TransportplanMeldungTyp>();
        }

        public virtual Transportplan ToEntity()
        {
            Transportplan tp = new Transportplan();
            tp.TpNr = this.TpNr;
            tp.SaNr = this.SaNr;
            tp.Status = this.Status;
            tp.Meldungen = this.Meldungen.Select(item => item).ToList();
            foreach (TransportplanSchrittDTO tpsDTO in this.TransportplanSchritte)
            {
                tp.TransportplanSchritte.Add(tpsDTO.ToEntity());
            }
            foreach (FrachteinheitDTO feDTO in this.Frachteinheiten)
            {
                tp.Frachteinheiten.Add(feDTO.ToEntity());
            }
            return tp;
        }
    }

    public class TransportplanSchrittDTO : DTOType<TransportplanSchrittDTO>
    {
        public virtual int TpsNr { get; set; }
        public virtual DateTime PlanStartzeit { get; set; }
        public virtual DateTime PlanEndezeit { get; set; }
        public virtual DateTime SpaetesterStart { get; set; }
        public virtual decimal Kosten { get; set; }

        public TransportplanSchrittDTO()
        {
        }

        public virtual TransportplanSchritt ToEntity()
        {
            TransportplanSchritt tps = new TransportplanSchritt();
            this.ToEntity(tps);
            return tps;
        }

        protected virtual void ToEntity(TransportplanSchritt tps)
        {
            tps.TpsNr = this.TpsNr;
            tps.PlanStartzeit = this.PlanStartzeit;
            tps.PlanEndezeit = this.PlanEndezeit;
            tps.SpaetesterStart = this.SpaetesterStart;
            tps.Kosten = this.Kosten;
        }
    }

    public class TransportAktivitaetDTO : TransportplanSchrittDTO
    {
        public virtual int VerwendeteKapazitaetTEU { get; set; }
        public virtual int VerwendeteKapazitaetFEU { get; set; }
        public virtual TimeSpan WartezeitAnStart { get; set; }
        public virtual int FrachtfuehrerRahmenvertrag { get; set; }
        public virtual int Frachtauftrag { get; set; }
        public virtual long FuerTransportAufTransportbeziehung { get; set; }

        public TransportAktivitaetDTO()
        {
        }

        public override TransportplanSchritt ToEntity()
        {
            TransportAktivitaet ta = new TransportAktivitaet();
            base.ToEntity(ta);
            ta.VerwendeteKapazitaetTEU = this.VerwendeteKapazitaetTEU;
            ta.VerwendeteKapazitaetFEU = this.VerwendeteKapazitaetFEU;
            ta.WartezeitAnStart = this.WartezeitAnStart;
            ta.FrachtfuehrerRahmenvertrag = this.FrachtfuehrerRahmenvertrag;
            ta.Frachtauftrag = this.Frachtauftrag;
            ta.FuerTransportAufTransportbeziehung = this.FuerTransportAufTransportbeziehung;
            return ta;
        }
    }
}
