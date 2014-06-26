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
    public class Zahlungseingang : ICanConvertToDTO<ZahlungseingangDTO>
    {
        public virtual int ZahlungsNr { get; set; }
        public virtual WaehrungsType Zahlungsbetrag { get; set; }
        public virtual int KrNr { get; set; }

        public Zahlungseingang()
        {
        }

        public virtual ZahlungseingangDTO ToDTO()
        {
            ZahlungseingangDTO zeDTO = new ZahlungseingangDTO();
            zeDTO.ZahlungsNr = this.ZahlungsNr;
            zeDTO.Zahlungsbetrag = this.Zahlungsbetrag;
            zeDTO.KrNr = this.KrNr;
            return zeDTO;
        }

        internal class ZahlungseingangMap : ClassMap<Zahlungseingang>
        {
            public ZahlungseingangMap()
            {
                this.Id(x => x.ZahlungsNr);
                this.Map(x => x.Zahlungsbetrag);
                this.Map(x => x.KrNr);
            }
        }
    }
}
