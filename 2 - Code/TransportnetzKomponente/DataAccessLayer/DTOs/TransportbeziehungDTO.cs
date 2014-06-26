using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using Util.Common.Interfaces;

namespace ApplicationCore.TransportnetzKomponente.DataAccessLayer
{
    public class TransportbeziehungDTO : DTOType<TransportbeziehungDTO>, ICanConvertToEntity<Transportbeziehung>
    {
        public long TbNr { get; set; }
        public uint DistanzInKm { get; set; }
        public LokationDTO Start { get; private set; }
        public LokationDTO Ziel { get; private set; }

        public TransportbeziehungDTO(LokationDTO start, LokationDTO ziel)
        {
            Contract.Requires(start.LokNr >= 0);
            Contract.Requires(ziel.LokNr >= 0);

            this.TbNr = -1;
            this.Start = start;
            this.Ziel = ziel;
        }

        public override string ToString()
        {
            return Start.Name + "-(" + TbNr + ")->" + Ziel.Name;
        }

        public virtual Transportbeziehung ToEntity()
        {
            Transportbeziehung tb = new Transportbeziehung(this.Start.ToEntity(), this.Ziel.ToEntity());
            tb.TbNr = this.TbNr;
            tb.DistanzInKm = this.DistanzInKm;
            return tb;
        }
    }
}
