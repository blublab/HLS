using System;
using Util.Common.Interfaces;

namespace ApplicationCore.TransportnetzKomponente.DataAccessLayer
{
    public class Lokation : ICanConvertToDTO<LokationDTO>
    {
        public long LokNr { get; set; }
        public string Name { get; set; }
        public decimal LagerKostenProStunde { get; set; }
        public TimeSpan MaxLagerZeit { get; set; }

        public Lokation()
        {
        }

        public Lokation(string name, TimeSpan maxLagerZeit, decimal lagerKostenProStunde)
        {
            this.Name = name;
            this.MaxLagerZeit = maxLagerZeit;
            this.LagerKostenProStunde = lagerKostenProStunde;
        }

        public override string ToString()
        {
            return Name + "(" + LokNr + ")";
        }

        public virtual LokationDTO ToDTO()
        {
            LokationDTO lokDTO = new LokationDTO(this.Name, this.MaxLagerZeit, this.LagerKostenProStunde);
            lokDTO.LokNr = this.LokNr;
            return lokDTO;
        }
    }
}
