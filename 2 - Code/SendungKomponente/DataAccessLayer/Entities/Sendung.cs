using ApplicationCore.SendungKomponente.DataAccessLayer;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util.Common.Interfaces;

namespace ApplicationCore.SendungKomponente.DataAccessLayer
{
    public enum SendungsverfolgungsereignisArtTyp { Eingang, Ausgang, ZielErreicht }

    public class Sendung : ICanConvertToDTO<SendungDTO>
    {
        public virtual int SndNr { get; set; }
        public virtual int SaNr { get; set; }
        public virtual int TpNr { get; set; }
        public virtual List<int> FraeNr { get; set; }
        public virtual IList<Sendungsverfolgungsereignis> SveLst { get; set; }

        public Sendung()
        {
            this.SveLst = new List<Sendungsverfolgungsereignis>();
            this.FraeNr = new List<int>();
        }

        public virtual SendungDTO ToDTO()
        {
            SendungDTO sndDTO = new SendungDTO();
            sndDTO.SndNr = this.SndNr;
            sndDTO.SaNr = this.SaNr;
            sndDTO.TpNr = this.TpNr;
            sndDTO.FraeNr = this.FraeNr;
            sndDTO.SveLst = this.SveLst;
            return sndDTO;
        }
    }

    public class Sendungsverfolgungsereignis : ICanConvertToDTO<SendungsverfolgungsereignisDTO>
    {
        public virtual int Id { get; set; }
        public virtual DateTime Zeitpunkt { get; set; }
        public virtual SendungsverfolgungsereignisArtTyp Ereignisart { get; set; }
        public virtual string Nachrichteninhalt { get; set; }
        public virtual long Ort { get; set; }
        public virtual int SNr { get; set; }

        public Sendungsverfolgungsereignis()
        {
        }

        public virtual SendungsverfolgungsereignisDTO ToDTO()
        {
            SendungsverfolgungsereignisDTO sndveDTO = new SendungsverfolgungsereignisDTO();
            sndveDTO.Zeitpunkt = this.Zeitpunkt;
            sndveDTO.Ereignisart = this.Ereignisart;
            sndveDTO.Nachrichteninhalt = this.Nachrichteninhalt;
            sndveDTO.Ort = this.Ort;
            sndveDTO.SNr = this.SNr;
            sndveDTO.Id = this.Id;
            return sndveDTO;
        }
    }

    internal class SendungMap : ClassMap<Sendung>
    {
        public SendungMap()
        {
            this.Id(x => x.SndNr);
            this.Map(x => x.SaNr);
            this.Map(x => x.TpNr);
            this.Map(x => x.FraeNr);
            this.HasMany(x => x.SveLst).Cascade.All().Not.LazyLoad();
        }
    }

    internal class SendungsverfolgungsereignisMap : ClassMap<Sendungsverfolgungsereignis>
    {
        public SendungsverfolgungsereignisMap()
        {
            this.Id(x => x.Id);
            this.Map(x => x.Zeitpunkt);
            this.Map(x => x.Ereignisart);
            this.Map(x => x.Nachrichteninhalt);
            this.Map(x => x.Ort);
            this.Map(x => x.SNr);
        }
    }
}
