using ApplicationCore.GeschaeftspartnerKomponente.DataAccessLayer;
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
    public class AdresseDTO : DTOType<AdresseDTO>, ICanConvertToEntity<Adresse>
    {
        public virtual int Id { get; set; }
        public virtual string Strasse { get; set; }
        public virtual string Hausnummer { get; set; }
        public virtual string PLZ { get; set; }
        public virtual string Wohnort { get; set; }
        public virtual string Land { get; set; }

        public AdresseDTO()
        {
        }

        public virtual Adresse ToEntity()
        {
            Adresse a = new Adresse();
            a.Id = this.Id;
            a.Strasse = this.Strasse;
            a.Hausnummer = this.Hausnummer;
            a.PLZ = this.PLZ;
            a.Wohnort = this.Wohnort;
            a.Land = this.Land;
            return a;
        }
    }
}
