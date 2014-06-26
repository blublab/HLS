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
    public class GeschaeftspartnerDTO : DTOType<GeschaeftspartnerDTO>, ICanConvertToEntity<Geschaeftspartner>
    {
        public int GpNr { get; set; }
        public EMailType Email { get; set; }
        public string Nachname { get; set; }
        public string Vorname  { get; set; }
        public long Version { get; set; }
        public IList<AdresseDTO> Adressen { get; set; }

        public GeschaeftspartnerDTO()
        {
            this.Adressen = new List<AdresseDTO>();
        }

        public virtual Geschaeftspartner ToEntity()
        {
            Geschaeftspartner gp = new Geschaeftspartner();
            gp.GpNr = this.GpNr;
            gp.Email = this.Email;
            gp.Vorname = this.Vorname;
            gp.Nachname = this.Nachname;
            gp.Version = this.Version;
            foreach (AdresseDTO aDTO in this.Adressen)
            {
                gp.Adressen.Add(aDTO.ToEntity());
            }
            return gp;
        }
    }
}
