using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util.Common.Interfaces;

namespace ApplicationCore.SendungKomponente.DataAccessLayer
{
    public class SendungDTO : DTOType<SendungDTO>, ICanConvertToEntity<Sendung>
    {
        public virtual int SndNr { get; set; }
        public virtual int SaNr { get; set; }
        public virtual int TpNr { get; set; }
        public virtual List<int> FraeNr { get; set; }
        public virtual IList<Sendungsverfolgungsereignis> SveLst { get; set; }

        public SendungDTO()
        {
        }

        public virtual Sendung ToEntity()
        {
            Sendung snd = new Sendung();
            snd.SndNr = this.SndNr;
            snd.SaNr = this.SaNr;
            snd.TpNr = this.TpNr;
            snd.FraeNr = this.FraeNr;
            snd.SveLst = this.SveLst;
            return snd;
        }
    }

    public class SendungsverfolgungsereignisDTO : DTOType<SendungsverfolgungsereignisDTO>, ICanConvertToEntity<Sendungsverfolgungsereignis>
    {
        public virtual int Id { get; set; }
        public virtual DateTime Zeitpunkt { get; set; }
        public virtual SendungsverfolgungsereignisArtTyp Ereignisart { get; set; }
        public virtual string Nachrichteninhalt { get; set; }
        public virtual long Ort { get; set; }
        public virtual int SNr { get; set; }

        public SendungsverfolgungsereignisDTO()
        {
        }

        public virtual Sendungsverfolgungsereignis ToEntity()
        {
            Sendungsverfolgungsereignis sndve = new Sendungsverfolgungsereignis();
            sndve.Zeitpunkt = this.Zeitpunkt;
            sndve.Ereignisart = this.Ereignisart;
            sndve.Nachrichteninhalt = this.Nachrichteninhalt;
            sndve.Ort = this.Ort;
            sndve.SNr = this.SNr;
            sndve.Id = this.Id;
            return sndve;
        }
    }
}
