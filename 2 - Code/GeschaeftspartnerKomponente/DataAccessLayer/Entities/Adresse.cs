using Common.DataTypes;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util.Common.Interfaces;

namespace ApplicationCore.GeschaeftspartnerKomponente.DataAccessLayer
{
    public class Adresse : ICanConvertToDTO<AdresseDTO>
    {
        public virtual int Id { get; set; }
        public virtual string Strasse { get; set; }
        public virtual string Hausnummer { get; set; }
        public virtual string PLZ { get; set; }
        public virtual string Wohnort { get; set; }
        public virtual string Land { get; set; }

        public Adresse()
        {
        }

        public virtual AdresseDTO ToDTO()
        {
            AdresseDTO aDTO = new AdresseDTO();
            aDTO.Id = this.Id;
            aDTO.Strasse = this.Strasse;
            aDTO.Hausnummer = this.Hausnummer;
            aDTO.PLZ = this.PLZ;
            aDTO.Wohnort = this.Wohnort;
            aDTO.Land = this.Land;
            return aDTO;
        }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            Adresse p = obj as Adresse;
            if ((object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (Strasse == p.Strasse) && (Hausnummer == p.Hausnummer) 
                && (PLZ == p.PLZ) && (Wohnort == p.Wohnort) && (Land == p.Land);
        }

        public virtual bool Equals(Adresse p)
        {
            // If parameter is null return false:
            if ((object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (Strasse == p.Strasse) && (Hausnummer == p.Hausnummer)
                && (PLZ == p.PLZ) && (Wohnort == p.Wohnort) && (Land == p.Land);
        }

        public override int GetHashCode()
        {
            return Strasse.GetHashCode() ^ Hausnummer.GetHashCode() 
                ^ PLZ.GetHashCode() ^ Wohnort.GetHashCode() ^ Land.GetHashCode();
        }
    }

    internal class AdresseMap : ClassMap<Adresse>
    {
        public AdresseMap()
        {
            this.Id(x => x.Id);
            this.Map(x => x.Strasse);
            this.Map(x => x.Hausnummer);
            this.Map(x => x.PLZ);
            this.Map(x => x.Wohnort);
            this.Map(x => x.Land);
        }
    }
}
