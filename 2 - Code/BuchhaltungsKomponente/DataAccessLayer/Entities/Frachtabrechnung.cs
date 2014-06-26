using Common.DataTypes;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationCore.UnterbeauftragungKomponente.DataAccessLayer;
using Util.Common.Interfaces;

namespace ApplicationCore.BuchhaltungKomponente.DataAccessLayer
{
    public class Frachtabrechnung : ICanConvertToDTO<FrachtabrechnungDTO>
    {
        public virtual int RechnungsNr { get; set; }
        public virtual bool IstBestaetigt { get; set; }
        public virtual WaehrungsType Rechnungsbetrag { get; set; }
        public virtual int Frachtauftrag { get; set; }
        public virtual Gutschrift Gutschrift { get; set; }
        ////public virtual PDFTyp Inhalt { get; set; }

        public Frachtabrechnung()
        {
        }

        public virtual FrachtabrechnungDTO ToDTO()
        {
            FrachtabrechnungDTO faDTO = new FrachtabrechnungDTO();
            faDTO.RechnungsNr = this.RechnungsNr;
            faDTO.IstBestaetigt = this.IstBestaetigt;
            faDTO.Rechnungsbetrag = this.Rechnungsbetrag;
            faDTO.Frachtauftrag = this.Frachtauftrag;
            faDTO.Gutschrift = this.Gutschrift;
            ////fa.DTO.Inhalt = this.Inhalt;
            return faDTO;
        }
    }

    internal class FrachtabrechnungMap : ClassMap<Frachtabrechnung>
    {
        public FrachtabrechnungMap()
        {
            this.Id(x => x.RechnungsNr);
            this.Map(x => x.IstBestaetigt);
            this.Map(x => x.Rechnungsbetrag);
            this.Map(x => x.Frachtauftrag);
            this.References(x => x.Gutschrift).Cascade.All();
            ////this.Map(x => x.Inhalt).Not.Nullable();
        }
    }
}
