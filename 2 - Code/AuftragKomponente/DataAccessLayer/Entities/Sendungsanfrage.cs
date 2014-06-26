using ApplicationCore.GeschaeftspartnerKomponente.DataAccessLayer;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using Util.Common.Interfaces;

namespace ApplicationCore.AuftragKomponente.DataAccessLayer
{
    public enum SendungsanfrageStatusTyp { NichtErfasst, Erfasst, Geplant, Angenommen, InAusfuehrung, Abgelehnt, Abgelaufen, Abgeschlossen, Unterbrochen }

    public class Sendungsanfrage : ICanConvertToDTO<SendungsanfrageDTO>
    {
        public virtual int SaNr { get; set; }
        public virtual DateTime AbholzeitfensterStart { get; set; }
        public virtual DateTime AbholzeitfensterEnde { get; set; }
        public virtual DateTime AngebotGültigBis { get; set; }
        public virtual SendungsanfrageStatusTyp Status { get; protected internal set; }

        public virtual IList<Sendungsposition> Sendungspositionen { get; set; }
        public virtual long StartLokation { get; set; }
        public virtual long ZielLokation { get; set; }
        public virtual int SendungNr { get; set; }

        public virtual int Auftraggeber { get; set; }

        public Sendungsanfrage()
        {
            this.Sendungspositionen = new List<Sendungsposition>();
            this.Status = SendungsanfrageStatusTyp.NichtErfasst;
        }

        public virtual void UpdateStatus(SendungsanfrageStatusTyp neuerStatus)
        {
            bool übergangErlaubt;
            switch (this.Status)
            {
                case SendungsanfrageStatusTyp.NichtErfasst:
                    übergangErlaubt = neuerStatus == SendungsanfrageStatusTyp.Erfasst;
                    break;
                case SendungsanfrageStatusTyp.Erfasst:
                    übergangErlaubt = neuerStatus == SendungsanfrageStatusTyp.Geplant;
                    break;
                case SendungsanfrageStatusTyp.Geplant:
                    übergangErlaubt = new List<SendungsanfrageStatusTyp> { SendungsanfrageStatusTyp.Angenommen, SendungsanfrageStatusTyp.Abgelehnt, SendungsanfrageStatusTyp.Abgelaufen }.Contains(neuerStatus);
                    break;
                case SendungsanfrageStatusTyp.Abgelehnt:
                    übergangErlaubt = neuerStatus == SendungsanfrageStatusTyp.Erfasst;
                    break;
                case SendungsanfrageStatusTyp.Abgelaufen:
                    übergangErlaubt = neuerStatus == SendungsanfrageStatusTyp.Erfasst;
                    break;
                case SendungsanfrageStatusTyp.Angenommen:
                    übergangErlaubt = neuerStatus == SendungsanfrageStatusTyp.InAusfuehrung;
                    break;
                case SendungsanfrageStatusTyp.InAusfuehrung:
                    übergangErlaubt = new List<SendungsanfrageStatusTyp> { SendungsanfrageStatusTyp.Unterbrochen, SendungsanfrageStatusTyp.Abgeschlossen }.Contains(neuerStatus);
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
                throw new ArgumentException("Ungültiger Statusübergang für Sendungsanfrage. " + this.Status.ToString() + "-X->" + neuerStatus.ToString());
            } 
        }

        public virtual SendungsanfrageDTO ToDTO()
        {
            SendungsanfrageDTO saDTO = new SendungsanfrageDTO();
            saDTO.SaNr = this.SaNr;
            saDTO.AbholzeitfensterStart = this.AbholzeitfensterStart;
            saDTO.AbholzeitfensterEnde = this.AbholzeitfensterEnde;
            saDTO.AngebotGültigBis = this.AngebotGültigBis;
            saDTO.Status = this.Status;
            saDTO.StartLokation = this.StartLokation;
            saDTO.ZielLokation = this.ZielLokation;
            saDTO.SendungNr = this.SendungNr;
            foreach (Sendungsposition sp in this.Sendungspositionen)
            {
                saDTO.Sendungspositionen.Add(sp.ToDTO());
            }
            saDTO.Auftraggeber = this.Auftraggeber;
            return saDTO;
        }
    }

    public class Sendungsposition : ICanConvertToDTO<SendungspositionDTO>
    {
        public virtual int SendungspositionsNr { get; set; }
        public virtual decimal Bruttogewicht { get; set; }

        public Sendungsposition()
        {
        }

        public virtual SendungspositionDTO ToDTO()
        {
            SendungspositionDTO spDTO = new SendungspositionDTO();
            spDTO.SendungspositionsNr = this.SendungspositionsNr;
            spDTO.Bruttogewicht = this.Bruttogewicht;
            return spDTO;
        }
    }

    internal class SendungsanfrageMap : ClassMap<Sendungsanfrage>
    {
        public SendungsanfrageMap()
        {
            this.Id(x => x.SaNr);

            this.Map(x => x.AbholzeitfensterStart);
            this.Map(x => x.AbholzeitfensterEnde);
            this.Map(x => x.AngebotGültigBis);
            this.Map(x => x.Status);
            this.Map(x => x.StartLokation);
            this.Map(x => x.ZielLokation);
            this.Map(x => x.SendungNr);
            this.HasMany(x => x.Sendungspositionen).Cascade.All().Not.LazyLoad();
            this.Map(x => x.Auftraggeber);
        }
    }

    internal class SendungspositiontMap : ClassMap<Sendungsposition>
    {
        public SendungspositiontMap()
        {
            this.Id(x => x.SendungspositionsNr);

            this.Map(x => x.Bruttogewicht);
        }
    }
}
