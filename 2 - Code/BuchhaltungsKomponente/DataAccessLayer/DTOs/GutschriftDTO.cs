using ApplicationCore.BuchhaltungKomponente.DataAccessLayer;
using Common.DataTypes;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util.Common.Interfaces;

namespace ApplicationCore.BuchhaltungKomponente.DataAccessLayer
{
    public class GutschriftDTO : DTOType<GutschriftDTO>, ICanConvertToEntity<Gutschrift>
    {
        public int GutschriftNr { get; set; }
        public KontodatenType Kontodaten { get; set; }
        public WaehrungsType Betrag { get; set; }

        public virtual Gutschrift ToEntity()
        {
            Gutschrift g = new Gutschrift();
            g.GutschriftNr = this.GutschriftNr;
            g.Kontodaten = this.Kontodaten;
            g.Betrag = this.Betrag;
            return g;
        }
    }
}
