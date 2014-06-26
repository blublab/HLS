using System;
using Util.Common.Interfaces;

namespace ApplicationCore.TransportnetzKomponente.DataAccessLayer
{
    public class LokationDTO : DTOType<LokationDTO>, ICanConvertToEntity<Lokation>
    {
        public long LokNr { get; set; }
        public string Name { get; set; }
        public decimal LagerKostenProStunde { get; set; }
        public TimeSpan MaxLagerZeit { get; set; }

        public LokationDTO(string name, TimeSpan maxLagerZeit, decimal lagerKostenProStunde)
        {
            this.LokNr = -1;
            this.Name = name;
            this.MaxLagerZeit = maxLagerZeit;
            this.LagerKostenProStunde = lagerKostenProStunde;
        }

        public override string ToString()
        {
            return Name + "(" + LokNr + ")";
        }

        public virtual Lokation ToEntity()
        {
            Lokation lok = new Lokation();
            lok.LokNr = this.LokNr;
            lok.Name = this.Name;
            lok.LagerKostenProStunde = this.LagerKostenProStunde;
            lok.MaxLagerZeit = this.MaxLagerZeit;
            return lok;
        }
    }
}
