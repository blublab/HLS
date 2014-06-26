using ApplicationCore.GeschaeftspartnerKomponente.DataAccessLayer;
using System;
using System.Collections.Generic;
using Util.Common.Interfaces;

namespace ApplicationCore.AuftragKomponente.DataAccessLayer
{
    public class SendungsanfrageDTO : DTOType<SendungsanfrageDTO>, ICanConvertToEntity<Sendungsanfrage>
    {
        public int SaNr { get; set; }
        public DateTime AbholzeitfensterStart { get; set; }
        public DateTime AbholzeitfensterEnde { get; set; }
        public DateTime AngebotGültigBis { get; set; }
        public SendungsanfrageStatusTyp Status { get; set; }

        public IList<SendungspositionDTO> Sendungspositionen { get; set; }
        public long StartLokation { get; set; }
        public long ZielLokation { get; set; }
        public int SendungNr { get; set; }

        public int Auftraggeber { get; set; }

        public SendungsanfrageDTO()
        {
            this.Sendungspositionen = new List<SendungspositionDTO>();
            this.Status = SendungsanfrageStatusTyp.NichtErfasst;
        }

        public virtual Sendungsanfrage ToEntity()
        {
            Sendungsanfrage sa = new Sendungsanfrage();
            sa.SaNr = this.SaNr;
            sa.AbholzeitfensterStart = this.AbholzeitfensterStart;
            sa.AbholzeitfensterEnde = this.AbholzeitfensterEnde;
            sa.AngebotGültigBis = this.AngebotGültigBis;
            sa.Status = this.Status;
            sa.StartLokation = this.StartLokation;
            sa.ZielLokation = this.ZielLokation;
            sa.SendungNr = this.SendungNr;
            foreach (SendungspositionDTO spDTO in this.Sendungspositionen)
            {
                sa.Sendungspositionen.Add(spDTO.ToEntity());
            }
            sa.Auftraggeber = this.Auftraggeber;
            return sa;
        }

        public string ToString()
        {
            string result = "";
            result += "SaNr: " + SaNr;
            result += "\t";
            result += "AbhZtFnstrEnd :" + AbholzeitfensterEnde.ToString();
            result += "\t";
            result += "AbhZtFnstrStr :" + AbholzeitfensterStart.ToString();
            result += "\t";
            result += "AngbtGültBis :" + AngebotGültigBis.ToString();
            result += "\t";
            result += "Status :" + Status.ToString();
            result += "\t";
            result += "Start :" + StartLokation;
            result += "\t";
            result += "Ziel :" + ZielLokation;
            result += "\t";
            result += "AuftrgGbr :" + Auftraggeber;
            result += "\t";
            return result;
        }
    }

    public class SendungspositionDTO : DTOType<SendungspositionDTO>, ICanConvertToEntity<Sendungsposition>
    {
        public int SendungspositionsNr { get; set; }
        public decimal Bruttogewicht { get; set; }

        public virtual Sendungsposition ToEntity()
        {
            Sendungsposition sp = new Sendungsposition();
            sp.SendungspositionsNr = this.SendungspositionsNr;
            sp.Bruttogewicht = this.Bruttogewicht;
            return sp;
        }
    }
}
