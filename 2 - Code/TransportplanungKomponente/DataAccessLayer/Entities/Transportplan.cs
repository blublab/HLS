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
    [Serializable]
    public class TransportplanMeldungTyp : ValueType<TransportplanMeldungTyp>
    {
        public enum MeldungTag { AbholzeitUeberschritten }
        public readonly DateTime Zeitstempel;
        public readonly MeldungTag Tag;
        public readonly string Meldung;

        public TransportplanMeldungTyp(DateTime zeitstempel, MeldungTag tag, string meldung)
        {
            this.Zeitstempel = zeitstempel;
            this.Tag = tag;
            this.Meldung = meldung;
        }

        public override string ToString()
        {
            return this.Meldung;
        }
    }

    public enum TransportplanStatusTyp { Angelegt, Geplant, InAusfuehrung, Abgeschlossen }

    public class Transportplan : ICanConvertToDTO<TransportplanDTO>
    {
        public virtual int TpNr { get; set; }
        public virtual int SaNr { get; set; }
        public virtual TransportplanStatusTyp Status { get; set; }
        public virtual List<TransportplanMeldungTyp> Meldungen { get; set; }
        public virtual IList<TransportplanSchritt> TransportplanSchritte { get; set; }
        public virtual IList<Frachteinheit> Frachteinheiten { get; set; }

        public Transportplan()
        {
            this.Status = TransportplanStatusTyp.Angelegt;
            this.TransportplanSchritte = new List<TransportplanSchritt>();
            this.Frachteinheiten = new List<Frachteinheit>();
            this.Meldungen = new List<TransportplanMeldungTyp>();
        }

        public virtual decimal Kosten
        {
            get
            {
                decimal kosten = 0;
                foreach (TransportplanSchritt tps in this.TransportplanSchritte)
                {
                    kosten += tps.Kosten;
                }
                return kosten;
            }
        }

        public virtual TimeSpan Dauer
        {
            get
            {
                return this.LieferungAm - this.AbholungAm;
            }
        }

        public virtual DateTime AbholungAm
        {
            get
            {
                return this.TransportplanSchritte.First<TransportplanSchritt>().PlanStartzeit;
            }
        }

        public virtual DateTime LieferungAm
        {
            get
            {
                return this.TransportplanSchritte.Last<TransportplanSchritt>().PlanEndezeit;
            }
        }

        public virtual void UpdateStatus(TransportplanStatusTyp neuerStatus)
        {
            bool übergangErlaubt;
            switch (this.Status)
            {
                case TransportplanStatusTyp.Angelegt:
                    übergangErlaubt = neuerStatus == TransportplanStatusTyp.Geplant;
                    break;
                case TransportplanStatusTyp.Geplant:
                    übergangErlaubt = neuerStatus == TransportplanStatusTyp.InAusfuehrung;
                    break;
                case TransportplanStatusTyp.InAusfuehrung:
                    übergangErlaubt = neuerStatus == TransportplanStatusTyp.Abgeschlossen;
                    break;
                default:
                    übergangErlaubt = false;
                    break;
            }

            if (übergangErlaubt)
            {
                this.Status = neuerStatus;
            }
            else
            {
                throw new ArgumentException("Ungültiger Statusübergang für Transportplan. " + this.Status.ToString() + "-X->" + neuerStatus.ToString());
            }
        }

        public virtual TransportplanDTO ToDTO()
        {
            TransportplanDTO tpDTO = new TransportplanDTO();
            tpDTO.TpNr = this.TpNr;
            tpDTO.SaNr = this.SaNr;
            tpDTO.Status = this.Status;
            tpDTO.Kosten = this.Kosten;
            tpDTO.Dauer = this.Dauer;
            tpDTO.AbholungAm = this.AbholungAm;
            tpDTO.LieferungAm = this.LieferungAm;
            tpDTO.Meldungen = this.Meldungen.Select(item => item).ToList();
            foreach (TransportplanSchritt tps in this.TransportplanSchritte)
            {
                tpDTO.TransportplanSchritte.Add(tps.ToDTO());
            }
            foreach (Frachteinheit fe in this.Frachteinheiten)
            {
                tpDTO.Frachteinheiten.Add(fe.ToDTO());
            }
            return tpDTO;
        }
    }

    public class TransportplanSchritt : ICanConvertToDTO<TransportplanSchrittDTO>
    {
        public virtual int TpsNr { get; set; }
        public virtual DateTime PlanStartzeit { get; set; }
        public virtual DateTime PlanEndezeit { get; set; }
        public virtual DateTime SpaetesterStart { get; set; }
        public virtual decimal Kosten { get; set; }

        public TransportplanSchritt()
        {
        }

        public virtual TransportplanSchrittDTO ToDTO()
        {
            TransportplanSchrittDTO tpsDTO = new TransportplanSchrittDTO();
            this.ToDTO(tpsDTO);
            return tpsDTO;
        }

        protected virtual void ToDTO(TransportplanSchrittDTO tpsDTO)
        {
            tpsDTO.TpsNr = this.TpsNr;
            tpsDTO.PlanStartzeit = this.PlanStartzeit;
            tpsDTO.PlanEndezeit = this.PlanEndezeit;
            tpsDTO.SpaetesterStart = this.SpaetesterStart;
            tpsDTO.Kosten = this.Kosten;
        }
    }

    public class TransportAktivitaet : TransportplanSchritt
    {
        public virtual int VerwendeteKapazitaetTEU { get; set; }
        public virtual int VerwendeteKapazitaetFEU { get; set; }
        public virtual TimeSpan WartezeitAnStart { get; set; }
        public virtual int FrachtfuehrerRahmenvertrag { get; set; }
        public virtual int Frachtauftrag { get; set; }
        public virtual long FuerTransportAufTransportbeziehung { get; set; }

        public TransportAktivitaet()
        {
        }

        public override TransportplanSchrittDTO ToDTO()
        {
            TransportAktivitaetDTO taDTO = new TransportAktivitaetDTO();
            this.ToDTO(taDTO);
            return taDTO;
        }

        protected override void ToDTO(TransportplanSchrittDTO tpsDTO)
        {
            TransportAktivitaetDTO taDTO = tpsDTO as TransportAktivitaetDTO;
            Contract.Requires(taDTO != null, "Parameter muss ein TransportAktivitaetDTO sein.");

            base.ToDTO(taDTO);
            taDTO.VerwendeteKapazitaetTEU = this.VerwendeteKapazitaetTEU;
            taDTO.VerwendeteKapazitaetFEU = this.VerwendeteKapazitaetFEU;
            taDTO.WartezeitAnStart = this.WartezeitAnStart;
            taDTO.FrachtfuehrerRahmenvertrag = this.FrachtfuehrerRahmenvertrag;
            taDTO.Frachtauftrag = this.Frachtauftrag;
            taDTO.FuerTransportAufTransportbeziehung = this.FuerTransportAufTransportbeziehung;
        }
    }

    internal class TransportplanMap : ClassMap<Transportplan>
    {
        public TransportplanMap()
        {
            this.Id(x => x.TpNr);

            this.Map(x => x.SaNr);
            this.Map(x => x.Status);
            this.Map(x => x.Meldungen);
            this.HasMany(x => x.Frachteinheiten).Cascade.All().Not.LazyLoad();
            this.HasMany(x => x.TransportplanSchritte).Cascade.All().Not.LazyLoad();
        }
    }

    internal class TransportplanSchrittMap : ClassMap<TransportplanSchritt>
    {
        public TransportplanSchrittMap()
        {
            this.Id(x => x.TpsNr);

            this.Map(x => x.PlanStartzeit);
            this.Map(x => x.PlanEndezeit);
            this.Map(x => x.SpaetesterStart);
            this.Map(x => x.Kosten);
        }
    }

    internal class TransportAktivitaetMap : SubclassMap<TransportAktivitaet>
    {
        public TransportAktivitaetMap()
        {
            this.Map(x => x.FrachtfuehrerRahmenvertrag);
            this.Map(x => x.FuerTransportAufTransportbeziehung);
            this.Map(x => x.Frachtauftrag);
            this.Map(x => x.WartezeitAnStart);
            this.Map(x => x.VerwendeteKapazitaetTEU);
            this.Map(x => x.VerwendeteKapazitaetFEU);
        }
    }
}
