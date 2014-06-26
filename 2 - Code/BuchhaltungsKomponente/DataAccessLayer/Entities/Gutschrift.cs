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
    public class Gutschrift : ICanConvertToDTO<GutschriftDTO>
    {
        public virtual int GutschriftNr { get; set; }
        public virtual KontodatenType Kontodaten { get; set; }
        public virtual WaehrungsType Betrag { get; set; }

        public Gutschrift()
        {
        }

        public virtual GutschriftDTO ToDTO()
        {
            GutschriftDTO gDTO = new GutschriftDTO();
            gDTO.GutschriftNr = this.GutschriftNr;
            gDTO.Kontodaten = this.Kontodaten;
            gDTO.Betrag = this.Betrag;
            return gDTO;
        }

        public override bool Equals(object obj) 
        {
            Gutschrift other = obj as Gutschrift;

            if (this == obj) 
            {
                return true;
            }

            if (other == null)
            {
                return false;
            }

            return this.Betrag.Equals(other.Betrag) && this.GutschriftNr.Equals(other.GutschriftNr) && this.Kontodaten.Equals(other.Kontodaten);
        }

        public override int GetHashCode()
        {
            return this.Betrag.GetHashCode() + this.GutschriftNr.GetHashCode() + this.Kontodaten.GetHashCode();
        }

        internal class GutschriftMap : ClassMap<Gutschrift>
        {
            public GutschriftMap()
            {
                this.Id(x => x.GutschriftNr);
                this.Map(x => x.Kontodaten);
                this.Map(x => x.Betrag);
            }
        }
    }
}
