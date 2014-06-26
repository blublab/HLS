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
    public class ZahlungseingangDTO : DTOType<ZahlungseingangDTO>, ICanConvertToEntity<Zahlungseingang>
    {
        public int ZahlungsNr { get; set; }
        public WaehrungsType Zahlungsbetrag { get; set; }
        public int KrNr { get; set; }

        public virtual Zahlungseingang ToEntity()
        {
            Zahlungseingang ze = new Zahlungseingang();
            ze.ZahlungsNr = this.ZahlungsNr;
            ze.Zahlungsbetrag = this.Zahlungsbetrag;
            ze.KrNr = this.KrNr;
            return ze;
        }
    }
}
