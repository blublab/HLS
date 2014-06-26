using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApplicationCore.TransportplanungKomponente.DataAccessLayer
{
    public enum FrachteinheitTyp { TEU, FEU }

    public sealed class TEU
    {
        public const decimal LEERGEWICHT_TONS = 2.250m;
        public const decimal MAXZULADUNG_TONS = 21.750m;
        public const decimal MAX_TONS = 24m;
    }

    public sealed class FEU
    {
        public const decimal LEERGEWICHT_TONS = 3.780m;
        public const decimal MAXZULADUNG_TONS = 26.700m;
        public const decimal MAX_TONS = 30.480m;
    }

    public class Frachteinheit : ICloneable
    {
        public virtual int FraeNr { get; set; }
        public virtual FrachteinheitTyp FraeTyp { get; set; }

        public virtual List<int> Sendungspositionen { get; set; }

        public Frachteinheit()
        {
        }

        public Frachteinheit(FrachteinheitTyp fraeTyp)
        {
            this.FraeTyp = fraeTyp;
            this.Sendungspositionen = new List<int>();
        }

        public virtual FrachteinheitDTO ToDTO()
        {
            FrachteinheitDTO feDTO = new FrachteinheitDTO();
            feDTO.FraeNr = this.FraeNr;
            feDTO.FraeTyp = this.FraeTyp;
            feDTO.Sendungspositionen = this.Sendungspositionen.Select(item => item).ToList();
            return feDTO;
        }

        public virtual object Clone()
        {
            Frachteinheit frae = new Frachteinheit();
            frae.FraeNr = this.FraeNr;
            frae.FraeTyp = this.FraeTyp;
            frae.Sendungspositionen = this.Sendungspositionen.Select(item => item).ToList();
            return frae;
        }
    }

    internal class FrachteinheitMap : ClassMap<Frachteinheit>
    {
        public FrachteinheitMap()
        {
            this.Id(x => x.FraeNr);

            this.Map(x => x.FraeTyp);
            this.Map(x => x.Sendungspositionen);
        }
    }
}
