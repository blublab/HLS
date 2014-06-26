using ApplicationCore.BuchhaltungKomponente.DataAccessLayer;
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
    public class FrachtabrechnungDTO : DTOType<FrachtabrechnungDTO>, ICanConvertToEntity<Frachtabrechnung>
    {
        public virtual int RechnungsNr { get; set; }
        public virtual bool IstBestaetigt { get; set; }
        public virtual WaehrungsType Rechnungsbetrag { get; set; }
        public virtual int Frachtauftrag { get; set; }
        public virtual Gutschrift Gutschrift { get; set; }
        ////public virtual PDFTyp Inhalt { get; set; }

        public virtual Frachtabrechnung ToEntity()
        {
            Frachtabrechnung fa = new Frachtabrechnung();
            fa.RechnungsNr = this.RechnungsNr;
            fa.IstBestaetigt = this.IstBestaetigt;
            fa.Rechnungsbetrag = this.Rechnungsbetrag;
            fa.Frachtauftrag = this.Frachtauftrag;
            fa.Gutschrift = this.Gutschrift;
            ////fa.Inhalt = this.Inhalt;
            return fa;
        }
    }
}
