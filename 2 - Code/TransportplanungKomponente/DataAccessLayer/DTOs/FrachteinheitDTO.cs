using FluentNHibernate.Mapping;
using System.Collections.Generic;
using System.Linq;

namespace ApplicationCore.TransportplanungKomponente.DataAccessLayer
{
    public class FrachteinheitDTO
    {
        public virtual int FraeNr { get; set; }
        public virtual FrachteinheitTyp FraeTyp { get; set; }

        public virtual List<int> Sendungspositionen { get; set; }

        public FrachteinheitDTO()
        {
        }
   
        public virtual Frachteinheit ToEntity()
        {
            Frachteinheit fe = new Frachteinheit();
            fe.FraeNr = this.FraeNr;
            fe.FraeTyp = this.FraeTyp;
            fe.Sendungspositionen = this.Sendungspositionen.Select(item => item).ToList();
            return fe;
        }
    }
}
