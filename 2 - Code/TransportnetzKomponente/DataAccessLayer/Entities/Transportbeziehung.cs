using Neo4jClient;
using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using Util.Common.Interfaces;

namespace ApplicationCore.TransportnetzKomponente.DataAccessLayer
{
    public class Transportbeziehung :
        Relationship,
        IRelationshipAllowingSourceNode<Lokation>,
        IRelationshipAllowingTargetNode<Lokation>,
        ICanConvertToDTO<TransportbeziehungDTO>
    {
        public static readonly string TypeKey = "TRANSPORTBEZIEHUNG";
        public uint DistanzInKm { get; set; }
        public Lokation Start { get; set; }
        public Lokation Ziel { get; set; }
        public long TbNr { get; set; }

        public Transportbeziehung(Lokation start, Lokation ziel)
            : base(new NodeReference(ziel.LokNr))
        {
            Contract.Requires(start.LokNr >= 0);
            Contract.Requires(ziel.LokNr >= 0);

            Start = start;
            Ziel = ziel;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete]
        public Transportbeziehung()
            : base(0)
        {
            // benötigt für Neo4j
        }

        public override string RelationshipTypeKey
        {
            get { return TypeKey; }
        }

        public override string ToString()
        {
            return Start.Name + "-(" + TbNr + ")->" + Ziel.Name;
        }

        public virtual TransportbeziehungDTO ToDTO()
        {
            TransportbeziehungDTO tbDTO = new TransportbeziehungDTO(this.Start.ToDTO(), this.Ziel.ToDTO());
            tbDTO.TbNr = this.TbNr;
            tbDTO.DistanzInKm = this.DistanzInKm;
            return tbDTO;
        }
    }
}
